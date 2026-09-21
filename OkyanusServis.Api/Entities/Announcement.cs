namespace OkyanusServis.Api.Entities;

/// <summary>
/// Tüm velilere gönderilen toplu duyuru. "Announcements" tablosuna karşılık gelir.
/// </summary>
public class Announcement
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;   // duyuru metni
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
