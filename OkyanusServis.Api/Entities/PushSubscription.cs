namespace OkyanusServis.Api.Entities;

/// <summary>
/// Bir velinin tarayıcı/telefon push aboneliği. Veli "bildirime izin ver" dediğinde
/// tarayıcı bir abonelik üretir, biz de buraya kaydederiz. Bir velinin birden çok
/// cihazı (telefon + bilgisayar) olabilir → her cihaz ayrı satır.
/// "PushSubscriptions" tablosuna karşılık gelir.
/// </summary>
public class PushSubscription
{
    public int Id { get; set; }

    // Hangi kullanıcıya (veli hesabı) ait. IdentityUser.Id.
    public string UserId { get; set; } = string.Empty;

    // Tarayıcının verdiği push adresi ve şifreleme anahtarları.
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
