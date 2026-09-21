using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using WebPush;

namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>
/// Gerçek web push göndericisi. Bir kaydın veli hesabına bağlı tüm cihazlara
/// (tarayıcı push aboneliklerine) bildirim yollar. Süresi dolan abonelikleri temizler.
/// </summary>
public class WebPushSender : IPushSender
{
    private readonly AppDbContext _db;
    private readonly ILogger<WebPushSender> _log;

    public WebPushSender(AppDbContext db, ILogger<WebPushSender> log)
    {
        _db = db;
        _log = log;
    }

    public async Task SendToRegistrationAsync(int registrationId, string title, string message, CancellationToken ct = default)
    {
        var reg = await _db.Registrations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == registrationId, ct);
        if (reg?.UserId is null)
        {
            _log.LogInformation("Kayıt {Id} için veli hesabı yok, push atlanıyor.", registrationId);
            return;
        }

        await SendToUserAsync(reg.UserId, title, message, ct);
    }

    /// <summary>Bir kullanıcının tüm cihazlarına push gönderir (test ucundan da çağrılır).</summary>
    public async Task SendToUserAsync(string userId, string title, string message, CancellationToken ct = default)
    {
        var cfg = await _db.ServiceConfigs.AsNoTracking().FirstOrDefaultAsync(ct);
        if (cfg?.VapidPublicKey is null || cfg.VapidPrivateKey is null)
        {
            _log.LogWarning("VAPID anahtarları yok, push gönderilemedi.");
            return;
        }

        var subs = await _db.PushSubscriptions.Where(s => s.UserId == userId).ToListAsync(ct);
        if (subs.Count == 0)
        {
            _log.LogInformation("Kullanıcı {UserId} için push aboneliği yok.", userId);
            return;
        }

        var subject = string.IsNullOrWhiteSpace(cfg.VapidSubject)
            ? "mailto:info@okyanusservis.com" : cfg.VapidSubject;
        var vapid = new VapidDetails(subject, cfg.VapidPublicKey, cfg.VapidPrivateKey);
        var client = new WebPushClient();
        var payload = JsonSerializer.Serialize(new
        {
            title,
            body = message,
            icon = "/icon-192.png",
            url = "/portalim"
        });

        var expired = new List<Entities.PushSubscription>();
        foreach (var s in subs)
        {
            try
            {
                var sub = new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth);
                await client.SendNotificationAsync(sub, payload, vapid);
            }
            catch (WebPushException ex) when (
                ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                expired.Add(s); // abonelik ölmüş → temizle
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Push gönderilemedi (endpoint {Ep}).", s.Endpoint);
            }
        }

        if (expired.Count > 0)
        {
            _db.PushSubscriptions.RemoveRange(expired);
            await _db.SaveChangesAsync(ct);
            _log.LogInformation("{Count} ölü push aboneliği silindi.", expired.Count);
        }
    }
}
