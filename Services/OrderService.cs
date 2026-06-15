using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.Cloud.Firestore;
using haru.market.Models;
using Microsoft.Extensions.Configuration;
using Xendit.net;

namespace haru.market.Services
{
    public class OrderService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OrderService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();

            // Initialize Firestore
            string keyPath = "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json";
            var credential = Google.Apis.Auth.OAuth2.ServiceAccountCredential.FromServiceAccountData(
                new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(keyPath)))
            ).ToGoogleCredential();

            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = "haru-market",
                Credential = credential
            }.Build();

            // Initialize Xendit SDK
            XenditConfiguration.ApiKey = _configuration["Xendit:SecretKey"];
        }

        // US-09 & 011: Register complete order details to firestore
        public async Task<string> CreateOrderAsync(OrderPlacementViewModel orderData)
        {
            string generatedOrderId = await _firestoreDb.RunTransactionAsync(async transaction =>
            {
                double totalAmount = 0;

                foreach (var item in orderData.Items)
                {
                    DocumentReference productRef = _firestoreDb.Collection("products").Document(item.ProductId);
                    DocumentSnapshot productSnapshot = await transaction.GetSnapshotAsync(productRef);

                    if (!productSnapshot.Exists)
                    {
                        throw new Exception($"Product '{item.ProductName}' no longer exists in our catalog.");
                    }

                    long currentStock = productSnapshot.GetValue<long>("stockQuantity");
                    
                    if (currentStock < item.Quantity)
                    {
                        throw new Exception($"Insufficient stock for {item.ProductName}. Current: {currentStock}");
                    }

                    long updatedStockCount = currentStock - item.Quantity;
                    transaction.Update(productRef, "stockQuantity", updatedStockCount);
                    totalAmount += (double)item.Price * item.Quantity;
                }

                CollectionReference ordersCollection = _firestoreDb.Collection("orders");
                var firestoreOrderObject = new Dictionary<string, object>
                {
                    { "customerName", orderData.CustomerName },
                    { "shippingAddress", orderData.ShippingAddress },
                    { "customerEmail", orderData.CustomerEmail },
                    { "totalAmount", totalAmount },
                    { "status", "Pending" }, // Explicitly set initial status
                    { "paymentMethod", "Pending" }, // Will update after Xendit checkout
                    { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) },
                    { "items", orderData.Items.Select(i => new Dictionary<string, object>
                        {
                            { "productId", i.ProductId },
                            { "productName", i.ProductName },
                            { "price", Convert.ToDouble(i.Price) },
                            { "quantity", i.Quantity }
                        }).ToList() 
                    }
                };

                DocumentReference newOrderRef = await ordersCollection.AddAsync(firestoreOrderObject);
                return newOrderRef.Id; 
            });

            return generatedOrderId;
        }

        public async Task<string> CreateXenditInvoiceAsync(string databaseOrderId, OrderPlacementViewModel orderData)
        {
            try
            {
                double totalSum = orderData.Items.Sum(item => (double)item.Price * item.Quantity);

                var invoiceParams = new
                {
                    external_id = databaseOrderId,
                    amount = totalSum,
                    currency = "PHP",
                    payer_email = orderData.CustomerEmail,
                    description = $"Haru Market Checkout - Order #{databaseOrderId.Substring(0, 8)}",
                    invoice_duration = 86400,
                    payment_methods = new[] { "GCASH", "PAYMAYA", "GRABPAY", "CREDIT_CARD" },
                    success_redirect_url = "https://localhost:5167/Checkout/Success",
                    failure_redirect_url = "https://localhost:5167/Checkout/Cancel"
                };

                string xenditKey = _configuration["Xendit:SecretKey"] + ":";
                string base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(xenditKey));

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.xendit.co/v2/invoices");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
                request.Content = new StringContent(JsonSerializer.Serialize(invoiceParams), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using JsonDocument doc = JsonDocument.Parse(responseBody);
                    return doc.RootElement.GetProperty("invoice_url").GetString()!;
                }
                else
                {
                    Console.WriteLine($"[XENDIT ERROR] API Response: {responseBody}");
                    throw new Exception("Payment gateway rejected the invoice request.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[XENDIT ERROR] Execution failure: {ex.Message}");
                throw new Exception("Payment gateway initialization failed.");
            }
        }

        public async Task UpdateOrderStatusAsync(string orderId, string newStatus, string paymentChannel)
        {
            var orderRef = _firestoreDb.Collection("orders").Document(orderId);

            string polishedChannel = "Online";
            if (!string.IsNullOrWhiteSpace(paymentChannel))
            {
                switch (paymentChannel.ToUpper())
                {
                    case "CREDIT_CARD":
                        polishedChannel = "Credit Card";
                        break;
                    case "GCASH":
                        polishedChannel = "GCash";
                        break;
                    case "PAYMAYA":
                        polishedChannel = "Maya";
                        break;
                    case "GRABPAY":
                        polishedChannel = "GrabPay";
                        break;
                    default:
                        polishedChannel = char.ToUpper(paymentChannel[0]) + paymentChannel.Substring(1).ToLower();
                        break;
                }
            }

            var updates = new Dictionary<string, object>
            {
                { "status", newStatus },
                { "paymentMethod", polishedChannel }
            };

            await orderRef.UpdateAsync(updates);
        }

        public void DispatchInvoiceBackground(string orderId, OrderPlacementViewModel orderData)
        {
            Task.Run(async () =>
            {
                try
                {
                    double totalSum = orderData.Items.Sum(item => (double)item.Price * item.Quantity);
                    
                    StringBuilder itemRowsBuilder = new StringBuilder();
                    itemRowsBuilder.Append("<table style='width: 100%; border-collapse: collapse; margin-top: 10px;'>");
                    foreach (var item in orderData.Items)
                    {
                        itemRowsBuilder.Append($@"
                            <tr>
                                <td style='padding: 12px 0; border-bottom: 1px dashed #cccccc; font-size: 14px; font-weight: 500; color: #333333; text-align: left;'>
                                    {item.ProductName} <span style='color: #888888; font-size: 12px;'>x{item.Quantity}</span>
                                </td>
                                <td style='padding: 12px 0; border-bottom: 1px dashed #cccccc; font-size: 14px; font-weight: bold; color: #000000; text-align: right; width: 100px;'>
                                    ₱{item.Price * item.Quantity:N0}
                                </td>
                            </tr>");
                    }
                    itemRowsBuilder.Append("</table>");

                    string htmlEmailBody = $@"
                    <div style='font-family: ""Helvetica Neue"", Arial, sans-serif; background-color: #fbf9f6; padding: 30px 15px; text-align: left;'>
                        <div style='max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; padding: 25px; box-shadow: 0 4px 15px rgba(0,0,0,0.02);'>
                            
                            <div style='text-align: center; margin-bottom: 35px;'>
                                <img src='https://raw.githubusercontent.com/Zaewei/haru.market/refs/heads/main/wwwroot/images/header.png' alt='HARU Header' style='width: 100%; max-width: 550px; display: block; margin: 0 auto; border-radius: 12px;' />
                            </div>

                            <h2 style='color: #d63384; font-size: 28px; font-weight: bold; margin: 0 0 10px 0;'>Hi!</h2>
                            <p style='font-size: 20px; font-weight: bold; color: #000000; margin: 0 0 25px 0;'>Thanks for your purchase.</p>
                            
                            <hr style='border: none; border-top: 1px dashed #888888; margin: 20px 0;' />

                            <table style='width: 100%; border-collapse: collapse; font-size: 13px; color: #555555; margin-bottom: 15px;'>
                                <tr>
                                    <td style='vertical-align: top; padding: 0; text-align: left;'>
                                        <span style='display: block; margin-bottom: 4px; color: #777777;'>Order #</span>
                                        <strong style='color: #000000; font-size: 14px;'>{orderId.Substring(0, 8).ToUpper()}</strong>
                                    </td>
                                    <td style='text-align: right; vertical-align: top; padding: 0;'>
                                        <span style='display: block; margin-bottom: 4px; color: #777777;'>Placed on</span>
                                        <strong style='color: #000000; font-size: 14px;'>{DateTime.UtcNow:MMMM dd, yyyy}</strong>
                                    </td>
                                </tr>
                            </table>

                            <div style='margin: 20px 0;'>
                                {itemRowsBuilder.ToString()}
                            </div>

                            <hr style='border: none; border-top: 1px dashed #888888; margin: 25px 0;' />

                            <div style='padding-top: 5px;'>
                                <table style='width: 100%; border-collapse: collapse; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='font-size: 16px; font-weight: bold; color: #000000; text-align: left;'>
                                            Total Amount
                                        </td>
                                        <td style='font-size: 22px; font-weight: bold; color: #d63384; text-align: right;'>
                                            ₱{totalSum:N0}
                                        </td>
                                    </tr>
                                </table>
                                
                                <div style='text-align: center; margin-top: 15px;'>
                                    <img src='https://raw.githubusercontent.com/Zaewei/haru.market/refs/heads/main/wwwroot/images/footer.png' alt='HARU Footer' style='width: 100%; max-width: 550px; display: block; margin: 0 auto;' />
                                </div>
                            </div>

                        </div>
                    </div>";

                    var emailPayload = new
                    {
                        from = "onboarding@resend.dev",
                        to = new[] { orderData.CustomerEmail },
                        subject = $"Your HARU.market Purchase Receipt - #{orderId.Substring(0, 8).ToUpper()}",
                        html = htmlEmailBody
                    };

                    string resendApiKey = _configuration["Resend:ApiKey"] ?? "";
                    
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resendApiKey);
                    request.Content = new StringContent(JsonSerializer.Serialize(emailPayload), Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[RESEND ERROR] Failed to dispatch email: {errorResponse}");
                    }
                    else
                    {
                        Console.WriteLine($"[RESEND SUCCESS] Live template email sent to {orderData.CustomerEmail}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Background dispatch error telemetry alert: {ex.Message}");
                }
            });
        }
        public async Task<List<OrderViewModel>> GetAllOrdersAsync()
        {
            var ordersList = new List<OrderViewModel>();
            CollectionReference collection = _firestoreDb.Collection("orders");
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> data = document.ToDictionary();

                    ordersList.Add(new OrderViewModel
                    {
                        Id = document.Id,
                        Name = data.ContainsKey("customerName") ? data["customerName"].ToString()! : "Unknown Customer",
                        Email = data.ContainsKey("customerEmail") ? data["customerEmail"].ToString()! : "",
                        Status = data.ContainsKey("status") ? data["status"].ToString()! : "Pending",
                        PaymentMethod = data.ContainsKey("paymentMethod") ? data["paymentMethod"].ToString()! : "Gcash",
                        Total = data.ContainsKey("totalAmount") ? Convert.ToDecimal(data["totalAmount"]) : 0,
                        Date = data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts
                            ? ts.ToDateTime()
                            : DateTime.UtcNow
                    });
                }
            }

            return ordersList.OrderByDescending(o => o.Date).ToList();
        }
    }
}