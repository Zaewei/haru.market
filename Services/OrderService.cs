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
        public async Task<string> CreateOrderAsync(OrderPlacementViewModel orderData)
        {
            CollectionReference ordersCollection = _firestoreDb.Collection("orders");

            // calculate totals based on the items
            double totalAmount = orderData.Items.Sum(item => (double)item.Price * item.Quantity);

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

            DocumentReference docRef = await ordersCollection.AddAsync(firestoreOrderObject);
            return docRef.Id; // returns the order fulfilment token
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

                    // testing mailkit to send an actual email, very
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