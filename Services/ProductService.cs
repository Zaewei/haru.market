using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using haru.market.Models;

namespace haru.market.Services
{
    public class ProductService
    {
        private readonly FirestoreDb _firestoreDb;

        public ProductService()
        {
            // points to the key file for firebase admin auth, very neat
            string keyPath = "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json";

            // loads the service account credentials from the file path for authentication
            GoogleCredential credential = GoogleCredential.FromFile(keyPath);

            // initializes the firebase db connection using the project id and the loaded credentials
            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = "haru-market",
                Credential = credential
            }.Build();
        }

        public async Task<string> AddProductAsync(ProductViewModel product)
        {
            CollectionReference collection = _firestoreDb.Collection("products");

            Dictionary<string, object> productData = new Dictionary<string, object>
            {
                { "name", product.Name },
                { "description", product.Description },
                { "price", (double)product.Price }, 
                { "stockQuantity", product.StockQuantity },
                { "imageUrl", string.IsNullOrEmpty(product.ImageUrl) ? "placeholder.png" : product.ImageUrl },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };

            DocumentReference docRef = await collection.AddAsync(productData);
            return docRef.Id;
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            var productsList = new List<ProductViewModel>();
            
            // targets the collection named "products" in the firebase db
            CollectionReference collection = _firestoreDb.Collection("products");
            
            // grabs everything in that collection
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    // extract the data
                    Dictionary<string, object> data = document.ToDictionary();

                    // maps the data to the product view model and adds it to the list with basic checks for any missing fields
                    productsList.Add(new ProductViewModel
                    {
                        Name = data.ContainsKey("name") ? data["name"].ToString()! : "Unknown Product",
                        Description = data.ContainsKey("description") ? data["description"].ToString()! : "",
                        Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"]) : 0.00m,
                        StockQuantity = data.ContainsKey("stockQuantity") ? Convert.ToInt32(data["stockQuantity"]) : 0,
                        ImageUrl = data.ContainsKey("imageUrl") ? data["imageUrl"].ToString()! : "placeholder.png"
                    });
                }
            }

            return productsList;
        }
    }
}