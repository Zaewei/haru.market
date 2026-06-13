using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// this is the firebase admin sdk 
if (FirebaseApp.DefaultInstance == null)
{
    string keyPath = "haru-market-firebase-adminsdk-fbsvc-6e0cac4990.json";
    
    string jsonContent = System.IO.File.ReadAllText(keyPath);
    var credential = ServiceAccountCredential.FromServiceAccountData(
        new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent))
    ).ToGoogleCredential();

    builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
    {
        Credential = credential
    }));

    builder.Services.AddSingleton(provider =>
    {
        var firestoreBuilder = new FirestoreDbBuilder
        {
            ProjectId = "haru-market",
            Credential = credential
        };

        return firestoreBuilder.Build();
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
