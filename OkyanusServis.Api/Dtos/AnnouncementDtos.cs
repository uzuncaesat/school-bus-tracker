using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Api.Dtos;

/// <summary>Duyuru çıkışı.</summary>
public class AnnouncementDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Yeni duyuru girişi.</summary>
public class AnnouncementCreateDto
{
    [Required, MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
}
