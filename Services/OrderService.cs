using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using haru.market.Models;

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
    }
}