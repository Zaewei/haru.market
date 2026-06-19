using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuestPDF.Infrastructure;

string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

if (FirebaseApp.DefaultInstance == null)
{
    var googleCred = GoogleCredential.GetApplicationDefault();

    builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
    {
        Credential = googleCred 
    }));

    builder.Services.AddSingleton(provider =>
    {
        var firestoreBuilder = new FirestoreDbBuilder
        {
            ProjectId = "haru-market",
            Credential = googleCred
        };

        return firestoreBuilder.Build();
    });
}

// Add services to the container
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Fallback redirection path for unauthorized gates
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Keeps the user logged in for a week
        options.SlidingExpiration = true; // Refreshes the week-long timer automatically while active
        options.Cookie.HttpOnly = true;
    });

// Add session support for storing user authentication state and data
builder.Services.AddDistributedMemoryCache();

// Configure session options, such as the idle timeout duration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Register the services into the dependency container
builder.Services.AddSingleton<haru.market.Services.ProductService>();
builder.Services.AddSingleton<haru.market.Services.TrackingMoreService>();
builder.Services.AddSingleton<haru.market.Services.OrderService>();
builder.Services.AddSingleton<haru.market.Services.LookbookService>();
builder.Services.AddSingleton<haru.market.Services.UserService>();
builder.Services.AddSingleton<haru.market.Services.AuthService>();
builder.Services.AddSingleton<haru.market.Services.FavoritesService>();

var app = builder.Build();

// HTTP request confirmation
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 

app.UseRouting();

app.UseSession();
app.UseAuthentication(); // Identifies WHO the user is
app.UseAuthorization();  // Evaluates WHAT the user can access

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
