namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>
/// Bir veliye tarayıcı/telefon push bildirimi gönderir. Faz 4'te web push (VAPID)
/// implementasyonu gelecek; şimdilik log'layan stub kullanılıyor.
/// </summary>
public interface IPushSender
{
    Task SendToRegistrationAsync(int registrationId, string title, string message, CancellationToken ct = default);
}

/// <summary>Geçici: gerçek push yerine sadece log yazar. Faz 4'te değiştirilecek.</summary>
public class LogPushSender : IPushSender
{
    private readonly ILogger<LogPushSender> _log;
    public LogPushSender(ILogger<LogPushSender> log) => _log = log;

    public Task SendToRegistrationAsync(int registrationId, string title, string message, CancellationToken ct = default)
    {
        _log.LogInformation("[PUSH stub] Kayıt {Id} → {Title}: {Message}", registrationId, title, message);
        return Task.CompletedTask;
    }
}
