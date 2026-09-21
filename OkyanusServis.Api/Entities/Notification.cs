namespace OkyanusServis.Api.Entities;

/// <summary>
/// Bir kayda (öğrenciye/veliye) giden bildirim: "servise bindi", "indi",
/// "ödeme hatırlatması" vb. "Notifications" tablosuna karşılık gelir.
/// </summary>
public class Notification
{
    public int Id { get; set; }

    // Bu bildirim hangi kayda ait? (bir kayıt -> çok bildirim)
    public int RegistrationId { get; set; }              // FK
    public Registration? Registration { get; set; }      // navigation

    public string? Icon { get; set; }                    // emoji/simge (🚌, 🏫 ...)
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Bildirim türü: "Yaklaşım", "Ödeme", "Duyuru" vb. Filtreleme için.
    public string? Type { get; set; }
    // Veli bildirimi gördü mü? Okunmadı rozeti için.
    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
