using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Api.Dtos;

/// <summary>Bildirim çıkışı (veli portalında görünen mesajlar).</summary>
public class NotificationDto
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public string? Icon { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Yeni bildirim girişi (bindi/indi, hatırlatma vb.).</summary>
public class NotificationCreateDto
{
    [Required]
    public int RegistrationId { get; set; }
    public string? Icon { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;
}
