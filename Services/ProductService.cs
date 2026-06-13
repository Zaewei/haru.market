using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using haru.market.Models;

namespace haru.market.Services
{
    public class ProductService
    {
        private readonly FirestoreDb _firestoreDb;

        public ProductService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
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

                    productsList.Add(new ProductViewModel
                    {
                        Id = document.Id,
                        Name = data.ContainsKey("name") ? data["name"].ToString()! : "Unknown Product",
                        Description = data.ContainsKey("description") ? data["description"].ToString()! : "",
                        Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"]) : 0.00m,
                        StockQuantity = data.ContainsKey("stockQuantity") ? Convert.ToInt32(data["stockQuantity"]) : 0,
                        ImageUrl = data.ContainsKey("imageUrl") ? data["imageUrl"].ToString()! : "placeholder.png",
                        
                        CreatedAt = data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts
                            ? ts.ToDateTime()
                            : DateTime.UtcNow
                    });
                }
            }

            return productsList;
        }

        public async Task<string> GetUserRoleAsync(string email)
        {
            try
            {
                // user's role is stored in the users collection in firestore
                DocumentReference docRef = _firestoreDb.Collection("users").Document(email);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists && snapshot.ContainsField("role"))
                {
                    return snapshot.GetValue<string>("role");
                }
            }
            catch (Exception ex)
            {
                // fallback error
                Console.WriteLine($"Error fetching user role: {ex.Message}");
            }

            return "Customer";
        }

        public async Task<string?> GetEmailByFullNameAsync(string fullName)
        {
            try
            {
                Query query = _firestoreDb.Collection("users").WhereEqualTo("fullname", fullName).Limit(1);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Documents.Count > 0)
                {
                    DocumentSnapshot userDoc = snapshot.Documents[0];
                    if (userDoc.ContainsField("email"))
                    {
                        return userDoc.GetValue<string>("email");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding email by name tracking: {ex.Message}");
            }

            return null;
        }

        public async Task<int> GetTotalProductsCountAsync()
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("products");
                AggregateQuerySnapshot snapshot = await collection.Count().GetSnapshotAsync();
                return (int)(snapshot.Count ?? 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product aggregates: {ex.Message}");
                return 0;
            }
        }
        public async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("users");
                AggregateQuerySnapshot snapshot = await collection.Count().GetSnapshotAsync();
                return (int)(snapshot.Count ?? 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user aggregates: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTotalProductViewsCountAsync()
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("products");
                QuerySnapshot snapshot = await collection.GetSnapshotAsync();

                int totalViews = 0;

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists && document.ContainsField("views"))
                    {
                        totalViews += Convert.ToInt32(document.GetValue<object>("views"));
                    }
                }

                return totalViews;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching total product views: {ex.Message}");
                return 0;
            }
        }
    }
}