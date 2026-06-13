using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// 🚀 FORCE ENVIROMENT AUTHENTICATION IMMEDIATELY
string keyPath = "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json";
string fullPath = Path.Combine(Directory.GetCurrentDirectory(), keyPath);
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);

// Initialize Firebase Admin SDK using the newly set environment variable
if (FirebaseApp.DefaultInstance == null)
{
    builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.GetApplicationDefault() // Automatically pulls from environment variable
    }));

    // Register FirestoreDb using the exact same verified credential path
    builder.Services.AddSingleton(provider =>
    {
        var firestoreBuilder = new FirestoreDbBuilder
        {
            ProjectId = "haru-market" // Make sure this matches your Firebase Project ID exactly
        };

        return firestoreBuilder.Build(); // Automatically reads GOOGLE_APPLICATION_CREDENTIALS
    });
}

// add services to the container
builder.Services.AddControllersWithViews();

// add session support for storing user authentication state and data
builder.Services.AddDistributedMemoryCache();

// configure session options, such as the idle timeout duration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// register the product service as a singleton so it can be injected into the controllers when needed
builder.Services.AddSingleton<haru.market.Services.ProductService>();

// order processing
builder.Services.AddSingleton<haru.market.Services.OrderService>();

// lookbook service
builder.Services.AddSingleton<haru.market.Services.LookbookService>();

// auth service
builder.Services.AddSingleton<haru.market.Services.AuthService>();

var app = builder.Build();

// http request confirmation
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// enables the serving of css, js, and image files from the wwwroot folder
app.UseStaticFiles(); 

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    // register page as opener
    pattern: "{controller=Account}/{action=Register}/{id?}");

app.Run();
