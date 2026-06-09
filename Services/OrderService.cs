using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using haru.market.Models;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace haru.market.Services
{
    public class OrderService
    {
        private readonly FirestoreDb _firestoreDb;

        public OrderService()
        {
            string keyPath = "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json";
            var credential = Google.Apis.Auth.OAuth2.ServiceAccountCredential.FromServiceAccountData(
                new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(keyPath)))
            ).ToGoogleCredential();

            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = "haru-market",
                Credential = credential
            }.Build();
        }

        // register complete order details to firestore and return the doc id as order fulfillment token
        // this is us 09 and 011
        public async Task<string> CreateOrderAsync(OrderPlacementViewModel orderData)
        {
            // entire checkout process 
            string generatedOrderId = await _firestoreDb.RunTransactionAsync(async transaction =>
            {
                double totalAmount = 0;

                // verify stock for each item in order and update inventory counts
                foreach (var item in orderData.Items)
                {
                    DocumentReference productRef = _firestoreDb.Collection("products").Document(item.ProductId);
                    DocumentSnapshot productSnapshot = await transaction.GetSnapshotAsync(productRef);

                    if (!productSnapshot.Exists)
                    {
                        throw new Exception($"Product '{item.ProductName}' no longer exists in our market catalog repository.");
                    }

                    // reads the current stock quantity for the item from the db
                    long currentStock = productSnapshot.GetValue<long>("stockQuantity");
                    
                    if (currentStock < item.Quantity)
                    {
                        // stops the entire order if stock is insufficient for any item
                        throw new Exception($"Insufficient stock quantity available for {item.ProductName}. Current stock: {currentStock} units.");
                    }

                    // calculates the updated stock count after the purchase
                    long updatedStockCount = currentStock - item.Quantity;
                    
                    // updates the stock quantity in the database for each item
                    transaction.Update(productRef, "stockQuantity", updatedStockCount);

                    // total amount for oder
                    totalAmount += (double)item.Price * item.Quantity;
                }

                // if inventory checks pass then proceeed to create the order document
                CollectionReference ordersCollection = _firestoreDb.Collection("orders");
                var firestoreOrderObject = new Dictionary<string, object>
                {
                    { "customerName", orderData.CustomerName },
                    { "shippingAddress", orderData.ShippingAddress },
                    { "customerEmail", orderData.CustomerEmail },
                    { "totalAmount", totalAmount },
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
                return newOrderRef.Id; // commit the transaction and updates to the database, then returns the order document id
            });

            return generatedOrderId;
        }

        // this is the invoice for us 10
        public void DispatchInvoiceBackground(string orderId, OrderPlacementViewModel orderData)
        {
            Task.Run(() =>
            {
                try
                {
                    // totals
                    double totalSum = orderData.Items.Sum(item => (double)item.Price * item.Quantity);
                    
                    StringBuilder invoiceBuilder = new StringBuilder();
                    invoiceBuilder.AppendLine("==================================================");
                    invoiceBuilder.AppendLine("                HARU.MARKET RECEIPT               ");
                    invoiceBuilder.AppendLine("==================================================");
                    invoiceBuilder.AppendLine($"Order Reference Token: {orderId}");
                    invoiceBuilder.AppendLine($"Timestamp Generation : {DateTime.UtcNow} UTC");
                    invoiceBuilder.AppendLine("--------------------------------------------------");
                    invoiceBuilder.AppendLine($"Customer Name        : {orderData.CustomerName}");
                    invoiceBuilder.AppendLine($"Destination Delivery : {orderData.ShippingAddress}");
                    invoiceBuilder.AppendLine($"Profile Inbox Address: {orderData.CustomerEmail}");
                    invoiceBuilder.AppendLine("--------------------------------------------------");
                    invoiceBuilder.AppendLine("Line Items Purchased:");

                    foreach (var item in orderData.Items)
                    {
                        invoiceBuilder.AppendLine($" - {item.ProductName} (x{item.Quantity}) : ${item.Price * item.Quantity:F2}");
                    }

                    invoiceBuilder.AppendLine("--------------------------------------------------");
                    invoiceBuilder.AppendLine($"Total Amount Charged : ${totalSum:F2}");
                    invoiceBuilder.AppendLine("==================================================");
                    invoiceBuilder.AppendLine("   Thank you for shopping at haru.market!   ");
                    invoiceBuilder.AppendLine("==================================================");

                    // creats an invoice file in the base directory with the order id as part of the filename for retrieval
                    string outputFilename = $"Invoice_Receipt_{orderId}.txt";
                    string fullStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFilename);
                    
                    File.WriteAllText(fullStoragePath, invoiceBuilder.ToString());

                    // testing mailkit to send an actual email, very neat
                    var email = new MimeMessage();
                    email.From.Add(new MailboxAddress("haru.market Support", "support@haru.market"));
                    email.To.Add(new MailboxAddress(orderData.CustomerName, orderData.CustomerEmail));
                    email.Subject = $"Order Confirmation Receipt - #{orderId}";

                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.TextBody = invoiceBuilder.ToString();
                    email.Body = bodyBuilder.ToMessageBody();

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Connect("sandbox.smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);
                        
                        // mailkit sandox auth credentials
                        smtpClient.Authenticate("4ee893c2602ba1", "accb67817c94ac");
                        
                        smtpClient.Send(email);
                        smtpClient.Disconnect(true);
                    }
                }
                catch (Exception ex)
                {
                    // failsafe to log any errors during the process
                    Console.WriteLine($"Background dispatch error telemetry alert: {ex.Message}");
                }
            });
        }
    }
}