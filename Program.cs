using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

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
}

// add services to the container
builder.Services.AddControllersWithViews();

// register the product service as a singletoon so it can be injected into the controllers when needed
builder.Services.AddSingleton<haru.market.Services.ProductService>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();