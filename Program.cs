using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
var builder = WebApplication.CreateBuilder(args);

if (FirebaseApp.DefaultInstance == null)
{
    // 1. Force the app to parse the file into an object ONCE while the environment variable is still active
    var googleCred = GoogleCredential.GetApplicationDefault();

    // 2. Give that parsed memory object to Firebase
    builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
    {
        Credential = googleCred 
    }));

    // 3. Give the EXACT SAME parsed memory object to Firestore
    builder.Services.AddSingleton(provider =>
    {
        var firestoreBuilder = new FirestoreDbBuilder
        {
            ProjectId = "haru-market",
            Credential = googleCred // Zero obsolete warnings, and the connection can never drop!
        };

        return firestoreBuilder.Build();
    });
}
// Add services to the container
builder.Services.AddControllersWithViews();

// Add session support for storing user authentication state and data
builder.Services.AddDistributedMemoryCache();

// Configure session options, such as the idle timeout duration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Register the services into the dependency container
builder.Services.AddSingleton<haru.market.Services.ProductService>();
builder.Services.AddSingleton<haru.market.Services.OrderService>();
builder.Services.AddSingleton<haru.market.Services.LookbookService>();
builder.Services.AddSingleton<haru.market.Services.AuthService>();

var app = builder.Build();

// HTTP request confirmation
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enables the serving of css, js, and image files from the wwwroot folder
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