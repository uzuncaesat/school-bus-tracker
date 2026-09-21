using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>Servis saatleri yardımcısı: verilen ana göre aktif sefer (Sabah/Akşam) ya da null.</summary>
public static class ServiceHours
{
    public static string? CurrentLeg(ServiceConfig cfg, DateTime now)
    {
        var t = TimeOnly.FromDateTime(now);
        if (InWindow(t, cfg.MorningStart, cfg.MorningEnd)) return "Sabah";
        if (InWindow(t, cfg.EveningStart, cfg.EveningEnd)) return "Akşam";
        return null;
    }

    private static bool InWindow(TimeOnly now, string start, string end)
    {
        if (!TimeOnly.TryParse(start, out var s) || !TimeOnly.TryParse(end, out var e)) return false;
        return now >= s && now <= e;
    }
}
