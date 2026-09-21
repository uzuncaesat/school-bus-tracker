using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Web.Models;

/// <summary>Takip/yaklaşım ayarları (admin ekranı). VAPID private anahtarı burada yoktur.</summary>
public class ServiceConfigDto
{
    public bool Enabled { get; set; } = true;
    public bool UseMock { get; set; } = true;

    public string? ArventoUsername { get; set; }
    public string? ArventoPin1 { get; set; }
    public string? ArventoPin2 { get; set; }
    public string ArventoBaseUrl { get; set; } = "http://ws.arvento.com/v1/report.asmx";

    [Range(50, 100000)] public int ApproachThresholdMeters { get; set; } = 800;
    [Range(30, 600)] public int PollIntervalSeconds { get; set; } = 45;

    public string MorningStart { get; set; } = "06:45";
    public string MorningEnd { get; set; } = "09:00";
    public string EveningStart { get; set; } = "15:00";
    public string EveningEnd { get; set; } = "17:45";

    public bool VapidReady { get; set; }
}
