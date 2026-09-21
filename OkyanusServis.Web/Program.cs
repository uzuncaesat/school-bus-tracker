using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OkyanusServis.Web;
using OkyanusServis.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ---- Kimlik doğrulama servisleri ----
builder.Services.AddAuthorizationCore();
// Singleton: HttpClientFactory handler'ı ile AuthService AYNI token'ı paylaşsın
builder.Services.AddSingleton<AuthState>();
builder.Services.AddScoped<ApiAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthStateProvider>());
builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddScoped<AuthService>();

// ---- Ayarlar (wwwroot/appsettings.json'dan API adresi) ----
// Boşsa uygulamanın kendi adresi (aynı origin — API bu uygulamayı da servis ediyor).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl) || apiBaseUrl == "/")
    apiBaseUrl = builder.HostEnvironment.BaseAddress;
builder.Services.AddSingleton(new AppConfig { ApiBaseUrl = apiBaseUrl });

// ---- HttpClient (token başlığını ekleyen handler ile) ----
builder.Services.AddHttpClient("api", c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

// Kendi API istemcimiz (backend çağrılarını tek yerde toplar)
builder.Services.AddScoped<ApiClient>();

var host = builder.Build();

// Açılışta kayıtlı token varsa oturumu geri yükle
await host.Services.GetRequiredService<AuthService>().InitializeAsync();

await host.RunAsync();
