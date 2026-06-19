using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using haru.market.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace haru.market.Services
{
    public class ProductService
    {
        private readonly FirestoreDb _firestoreDb;

        public ProductService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        
        public FirestoreDb GetFirestoreDbInstance()
        {
            return _firestoreDb;
        }
        
        public async Task<string> AddProductAsync(ProductViewModel product)
        {
            CollectionReference collection = _firestoreDb.Collection("products");
            var productData = new Dictionary<string, object>
            {
                { "name", product.Name },
                { "description", product.Description ?? "" },
                { "price", (double)product.Price },
                { "stockQuantity", product.StockQuantity },
                { "color", product.Color ?? "" },
                { "size", product.Size ?? "" },
                { "imageUrl", string.IsNullOrEmpty(product.ImageUrl) ? "placeholder.png" : product.ImageUrl },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            return (await collection.AddAsync(productData)).Id;
        }

        public async Task DeleteProductAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("products").Document(id);
            await docRef.DeleteAsync();
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            var productsList = new List<ProductViewModel>();
            CollectionReference collection = _firestoreDb.Collection("products");
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> data = document.ToDictionary();

                    var stockDict = new Dictionary<string, int>();
                    if (data.ContainsKey("stockQuantity") && data["stockQuantity"] is Dictionary<string, object> rawStock)
                    {
                        stockDict = rawStock.ToDictionary(k => k.Key, v => Convert.ToInt32(v.Value));
                    }

                    productsList.Add(new ProductViewModel
                    {
                        Id = document.Id,
                        Name = data.ContainsKey("name") ? data["name"].ToString()! : "Unknown Product",
                        Description = data.ContainsKey("description") ? data["description"].ToString()! : "",
                        Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"]) : 0.00m,
                        StockQuantity = stockDict,
                        ImageUrl = GetImageUrlFromData(data),
                        Imageurl2 = data.ContainsKey("imageurl2") ? data["imageurl2"].ToString()! : "",
                        CreatedAt = data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts ? ts.ToDateTime() : DateTime.Now,
                        GroupKey = data.ContainsKey("groupKey") ? data["groupKey"].ToString()! : "",
                        Color = data.ContainsKey("color") ? data["color"].ToString()! : ""
                    });
                }
            }
            return productsList;
        }

        private string GetImageUrlFromData(Dictionary<string, object> data)
        {
            string[] possibleKeys = { "imageUrl", "Imageurl", "imageurl", "image", "image_url" };

            foreach (var key in possibleKeys)
            {
                if (data.ContainsKey(key) && data[key] != null)
                {
                    var val = data[key].ToString();
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            return "placeholder.png";
        }

        public async Task<ProductViewModel?> GetProductAsync(string productId)
        {
            DocumentSnapshot doc = await _firestoreDb.Collection("products").Document(productId).GetSnapshotAsync();

            if (!doc.Exists) return null;

            var data = doc.ToDictionary();
            
            var stockDict = new Dictionary<string, int>();
            if (data.ContainsKey("stockQuantity") && data["stockQuantity"] is Dictionary<string, object> rawStock)
            {
                stockDict = rawStock.ToDictionary(k => k.Key, v => Convert.ToInt32(v.Value));
            }

            return new ProductViewModel
            {
                Id = doc.Id,
                Name = data.ContainsKey("name") ? data["name"]?.ToString() ?? "" : "",
                Description = data.ContainsKey("description") ? data["description"]?.ToString() ?? "" : "",
                Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"]) : 0.00m,
                StockQuantity = stockDict,
                ImageUrl = data.ContainsKey("imageUrl") ? data["imageUrl"]?.ToString() ?? "" : "",
                Imageurl2 = data.ContainsKey("imageurl2") ? data["imageurl2"]?.ToString() ?? "" : "",
                GroupKey = data.ContainsKey("groupKey") ? data["groupKey"].ToString()! : "",
                Color = data.ContainsKey("color") ? data["color"].ToString()! : ""
            };
        }

        public async Task UpdateProductAsync(string id, string name, decimal price, string color, Dictionary<string, int> stockQuantity)
        {
            DocumentReference docRef = _firestoreDb.Collection("products").Document(id);
            
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "name", name },
                { "price", (double)price },
                { "color", color ?? "" },
                { "stockQuantity", stockQuantity } 
            };

            await docRef.UpdateAsync(updates);
        }

        public async Task<string> GetUserRoleAsync(string email)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection("users").Document(email);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists && snapshot.ContainsField("role")) return snapshot.GetValue<string>("role");
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching user role: {ex.Message}"); }
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
                    if (userDoc.ContainsField("email")) return userDoc.GetValue<string>("email");
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error finding email by name tracking: {ex.Message}"); }
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
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); return 0; }
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("users");
                AggregateQuerySnapshot snapshot = await collection.Count().GetSnapshotAsync();
                return (int)(snapshot.Count ?? 0);
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); return 0; }
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
                        totalViews += Convert.ToInt32(document.GetValue<object>("views"));
                }
                return totalViews;
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); return 0; }
        }

        public async Task AddToCartAsync(string uid, ProductViewModel product, string selectedSize, int requestedQuantity)
        {
            string cartDocId = $"{product.Id}_{selectedSize}";
            DocumentReference doc = _firestoreDb.Collection("users").Document(uid).Collection("cart").Document(cartDocId);
            
            DocumentSnapshot existing = await doc.GetSnapshotAsync();
            if (existing.Exists)
            {
                int currentQty = existing.GetValue<int>("quantity");
                await doc.UpdateAsync("quantity", currentQty + requestedQuantity);
                return;
            }

            var cartData = new Dictionary<string, object>
            {
                { "productId", product.Id ?? "" },
                { "productName", product.Name },
                { "price", (double)product.Price },
                { "imageUrl", product.ImageUrl },
                { "quantity", requestedQuantity },
                { "size", selectedSize },
                { "addedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await doc.SetAsync(cartData);
        }

        public async Task<List<CartItemViewModel>> GetCartItemsAsync(string uid)
        {
            var cartList = new List<CartItemViewModel>();
            try
            {
                CollectionReference cartRef = _firestoreDb.Collection("users").Document(uid).Collection("cart");
                QuerySnapshot snapshot = await cartRef.GetSnapshotAsync();

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var data = doc.ToDictionary();
                        cartList.Add(new CartItemViewModel
                        {
                            ProductId = doc.Id, 
                            ProductName = data.ContainsKey("productName") ? data["productName"]?.ToString() ?? "" : "Unknown Item",
                            Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"]) : 0.00m,
                            ImageUrl = data.ContainsKey("imageUrl") ? data["imageUrl"]?.ToString() ?? "placeholder.png" : "placeholder.png",
                            Quantity = data.ContainsKey("quantity") ? Convert.ToInt32(data["quantity"]) : 1,
                            Size = data.ContainsKey("size") ? data["size"]?.ToString() ?? "M" : "M" 
                        });
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            return cartList;
        }
    }
}