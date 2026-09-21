using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Api.Dtos;

/// <summary>Yaklaşım/takip ayarlarının okuma+yazma DTO'su. Gizli VAPID private anahtarı DIŞARI VERİLMEZ.</summary>
public class ServiceConfigDto
{
    public bool Enabled { get; set; } = true;
    public bool UseMock { get; set; } = true;

    [MaxLength(120)] public string? ArventoUsername { get; set; }
    [MaxLength(120)] public string? ArventoPin1 { get; set; }
    [MaxLength(120)] public string? ArventoPin2 { get; set; }
    [MaxLength(300)] public string ArventoBaseUrl { get; set; } = "http://ws.arvento.com/v1/report.asmx";

    [Range(50, 100000)] public int ApproachThresholdMeters { get; set; } = 800;
    [Range(30, 600)] public int PollIntervalSeconds { get; set; } = 45;

    public string MorningStart { get; set; } = "06:45";
    public string MorningEnd { get; set; } = "09:00";
    public string EveningStart { get; set; } = "15:00";
    public string EveningEnd { get; set; } = "17:45";

    // Sadece okunur — durumu göstermek için (push kuruldu mu?).
    public bool VapidReady { get; set; }
}
