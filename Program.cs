using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// this is the firebase admin sdk 
if (FirebaseApp.DefaultInstance == null)
{
    // Double-check your sidebar to ensure this matches your exact JSON filename!
    string keyPath = "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json";
    
    builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(keyPath)
    }));
}
// =========================================================================

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enables the serving of static visual layouts (like CSS, images, and JS files)
app.UseStaticFiles(); 

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();