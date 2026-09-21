using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>
/// Arka planda sürekli çalışan yaklaşım motoru. Servis saatlerinde araç konumlarını
/// çeker, her öğrencinin evine olan mesafeyi hesaplar ve eşiğin altına inince
/// (sefer başına bir kez) veliye "servis yaklaşıyor" bildirimi üretir.
/// </summary>
public class ApproachEngine : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ApproachEngine> _log;

    public ApproachEngine(IServiceProvider sp, ILogger<ApproachEngine> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Yaklaşım motoru başladı.");
        // Başlangıçta DB/seed hazır olsun diye kısa bekleme.
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            int waitSeconds = 45;
            try
            {
                waitSeconds = await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Yaklaşım turu hata verdi.");
            }
            await Task.Delay(TimeSpan.FromSeconds(Math.Max(30, waitSeconds)), stoppingToken);
        }
    }

    /// <summary>Tek bir tur. Sıradaki bekleme süresini (saniye) döner.</summary>
    private async Task<int> RunOnceAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var arvento = scope.ServiceProvider.GetRequiredService<IArventoClient>();
        var push = scope.ServiceProvider.GetRequiredService<IPushSender>();

        var cfg = await db.ServiceConfigs.AsNoTracking().FirstOrDefaultAsync(ct) ?? new ServiceConfig();
        if (!cfg.Enabled) return cfg.PollIntervalSeconds;

        // Şu an hangi sefer? (Sabah / Akşam / dışında)
        var leg = CurrentLeg(cfg, DateTime.Now);
        if (leg is null)
            return cfg.PollIntervalSeconds; // servis saati dışında, boşuna çalışma

        // Araçların konumu (node -> pozisyon)
        var positions = (await arvento.GetPositionsAsync(ct))
            .GroupBy(p => p.Node)
            .ToDictionary(g => g.Key, g => g.First());
        if (positions.Count == 0) return cfg.PollIntervalSeconds;

        // Bugün servise binecek, konumu ve aracı olan, bildirimi açık kayıtlar
        var today = DateOnly.FromDateTime(DateTime.Now);
        var regs = await db.Registrations
            .Include(r => r.Vehicle)
            .Where(r => r.NotifyOnApproach
                        && !r.SkipTomorrow
                        && r.Latitude != null && r.Longitude != null
                        && r.VehicleId != null
                        && r.Vehicle!.ArventoNode != null && r.Vehicle.ArventoNode != "")
            .ToListAsync(ct);

        // Bugün+bu sefer için zaten bildirilmiş kayıtlar (tekrarı engelle)
        var already = await db.ApproachAlerts
            .Where(a => a.Date == today && a.Leg == leg)
            .Select(a => a.RegistrationId)
            .ToListAsync(ct);
        var alerted = already.ToHashSet();

        int fired = 0;
        foreach (var r in regs)
        {
            if (alerted.Contains(r.Id)) continue;
            if (!positions.TryGetValue(r.Vehicle!.ArventoNode!, out var pos)) continue;

            double dist = GeoUtils.DistanceMeters(
                r.Latitude!.Value, r.Longitude!.Value, pos.Latitude, pos.Longitude);
            if (dist > cfg.ApproachThresholdMeters) continue;

            // Eşik altı → bildirim üret
            var title = "Servis yaklaşıyor 🚌";
            var msg = leg == "Sabah"
                ? $"{r.StudentName} için servis eve yaklaşıyor (~{dist:0} m). Lütfen hazır olun."
                : $"{r.StudentName} eve yaklaşıyor (~{dist:0} m). Servis birazdan orada.";

            db.Notifications.Add(new Notification
            {
                RegistrationId = r.Id,
                Icon = "🚌",
                Title = title,
                Message = msg,
                Type = "Yaklaşım",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            db.ApproachAlerts.Add(new ApproachAlert
            {
                RegistrationId = r.Id,
                VehicleId = r.VehicleId!.Value,
                Date = today,
                Leg = leg,
                DistanceMeters = dist,
                CreatedAt = DateTime.UtcNow
            });
            alerted.Add(r.Id);
            fired++;

            await push.SendToRegistrationAsync(r.Id, title, msg, ct);
        }

        if (fired > 0)
        {
            await db.SaveChangesAsync(ct);
            _log.LogInformation("{Count} yaklaşım bildirimi üretildi ({Leg}).", fired, leg);
        }

        return cfg.PollIntervalSeconds;
    }

    /// <summary>Verilen ana göre aktif sefer: "Sabah", "Akşam" ya da null (saat dışı).</summary>
    private static string? CurrentLeg(ServiceConfig cfg, DateTime now)
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
