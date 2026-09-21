namespace OkyanusServis.Web.Models;

/// <summary>API'den gelen duyuru.</summary>
public class AnnouncementDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Yeni duyuru girişi.</summary>
public class AnnouncementCreateDto
{
    public string Text { get; set; } = string.Empty;
}
