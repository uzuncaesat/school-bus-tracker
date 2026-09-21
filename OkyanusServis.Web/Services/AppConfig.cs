namespace OkyanusServis.Web.Services;

/// <summary>
/// Uygulama genel ayarları (wwwroot/appsettings.json'dan okunur).
/// API adresi tünel/LAN'a göre değiştiğinde tek yerden yönetilir.
/// </summary>
public class AppConfig
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5165/";
}
