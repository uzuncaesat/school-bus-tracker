using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Integrations.Arvento;

var builder = WebApplication.CreateBuilder(args);

// Windows servisi olarak çalışabilmesi için (oturum kapalıyken bile).
builder.Host.UseWindowsService();

// ---- Servisler (DI konteynerine eklenir) ----
builder.Services.AddControllers();          // Controller tabanlı API'yi etkinleştirir
builder.Services.AddEndpointsApiExplorer(); // Swagger'ın endpoint'leri keşfetmesi için
builder.Services.AddSwaggerGen();           // Swagger/OpenAPI dokümanını üretir

// EF Core: veritabanı olarak SQL Server (LocalDB) kullan, adresi appsettings'ten al
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ---- Kimlik doğrulama (ASP.NET Core Identity + token tabanlı API uçları) ----
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()   // /register, /login gibi token uçlarını sağlar
    .AddRoles<IdentityRole>()                   // rol desteği (Admin, Driver)
    .AddEntityFrameworkStores<AppDbContext>();  // kullanıcıları AppDbContext'te sakla

// ---- Arvento yaklaşım bildirimi entegrasyonu ----
builder.Services.AddHttpClient("arvento");                  // gerçek Arvento çağrıları için
builder.Services.AddScoped<IArventoClient, ArventoClient>(); // mock/gerçek konum kaynağı
builder.Services.AddScoped<WebPushSender>();                 // gerçek web push göndericisi
builder.Services.AddScoped<IPushSender>(sp => sp.GetRequiredService<WebPushSender>());
builder.Services.AddHostedService<ApproachEngine>();         // arka plan yaklaşım motoru

// CORS: Blazor frontend'inin (farklı port) API'ye erişmesine izin ver
const string FrontendCors = "frontend";
builder.Services.AddCors(options =>
{
    // Test/tünel için tüm kaynaklara izin. Token header ile taşındığından (cookie yok)
    // AllowAnyOrigin güvenlidir. Canlıya alırken burayı gerçek adrese daraltacağız.
    options.AddPolicy(FrontendCors, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Açılışta: bekleyen migration'ları uygula + başlangıç verisini ekle
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
    await IdentitySeeder.SeedAsync(sp);   // roller + yönetici hesabı

    // VAPID anahtarları yoksa bir kez üret ve sakla (web push için).
    var cfg = db.ServiceConfigs.First();
    if (string.IsNullOrEmpty(cfg.VapidPublicKey))
    {
        var keys = WebPush.VapidHelper.GenerateVapidKeys();
        cfg.VapidPublicKey = keys.PublicKey;
        cfg.VapidPrivateKey = keys.PrivateKey;
        db.SaveChanges();
    }
    // Eski satırlarda boş kalmış olabilir — VAPID subject'i garanti altına al.
    if (string.IsNullOrWhiteSpace(cfg.VapidSubject))
    {
        cfg.VapidSubject = "mailto:info@okyanusservis.com";
        db.SaveChanges();
    }
}

// ---- HTTP pipeline (her isteğin sırayla geçtiği hat) ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();     // JSON dokümanı: /swagger/v1/swagger.json
    app.UseSwaggerUI();   // Test arayüzü:  /swagger
}

app.UseHttpsRedirection();

// Blazor WASM uygulamasını da bu sunucu servis eder (tek origin, tek uygulama).
app.UseBlazorFrameworkFiles();   // _framework/*.wasm vb.
app.UseStaticFiles();            // index.html, css, js, appsettings.json ...

app.UseCors(FrontendCors);   // CORS, yönlendirmeden sonra - controller'lardan önce
app.UseAuthentication();     // "kimsin?" - token'ı çözer
app.UseAuthorization();      // "yetkin var mı?" - [Authorize] kontrolü
app.MapControllers();

// Identity token uçları: /api/auth/register, /api/auth/login ...
app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

// SPA yönlendirmesi: API/dosya olmayan tüm istekler Blazor'a (index.html) düşer.
app.MapFallbackToFile("index.html");

app.Run();
