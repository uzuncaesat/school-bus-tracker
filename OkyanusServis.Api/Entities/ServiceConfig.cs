namespace OkyanusServis.Api.Entities;

/// <summary>
/// Yaklaşım/takip sistemi ayarları. Tek satırlık config (Id = 1).
/// Admin panelinden düzenlenir. Arvento kimlik bilgileri ve eşikler burada.
/// </summary>
public class ServiceConfig
{
    public int Id { get; set; }

    // --- Arvento web servis erişimi ---
    public string? ArventoUsername { get; set; }
    public string? ArventoPin1 { get; set; }
    public string? ArventoPin2 { get; set; }
    // Arvento sunucu adresi (kendi sunucuları için ws.arvento.com, dış sunucu için IP/domain).
    public string ArventoBaseUrl { get; set; } = "http://ws.arvento.com/v1/report.asmx";

    // Gerçek Arvento yerine sahte (mock) konum üret. Bilgiler gelene kadar true.
    public bool UseMock { get; set; } = true;
    // Yaklaşım motoru açık mı?
    public bool Enabled { get; set; } = true;

    // --- Yaklaşım eşiği ---
    // Araç eve bu mesafeden (metre) yakınsa "yaklaşıyor" bildirimi gider.
    public int ApproachThresholdMeters { get; set; } = 800;
    // Konum kaç saniyede bir çekilsin (Arvento min 30 sn).
    public int PollIntervalSeconds { get; set; } = 45;

    // --- Servis saatleri (motor sadece bu aralıklarda çalışır, boşa istek harcamaz) ---
    // "HH:mm" formatında. Boşsa o pencere kapalı.
    public string MorningStart { get; set; } = "06:45";
    public string MorningEnd { get; set; } = "09:00";
    public string EveningStart { get; set; } = "15:00";
    public string EveningEnd { get; set; } = "17:45";

    // --- Web push (VAPID) ---
    // Açılışta boşsa otomatik üretilir. Public anahtar tarayıcıya gider, private gizli kalır.
    public string? VapidPublicKey { get; set; }
    public string? VapidPrivateKey { get; set; }
    public string VapidSubject { get; set; } = "mailto:info@okyanusservis.com";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
