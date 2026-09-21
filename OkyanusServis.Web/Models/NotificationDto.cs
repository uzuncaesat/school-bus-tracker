namespace OkyanusServis.Web.Models;

/// <summary>API'den gelen bildirim (veli portalında görünür).</summary>
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

/// <summary>Yeni bildirim gönderirken (bindi/indi).</summary>
public class NotificationCreateDto
{
    public int RegistrationId { get; set; }
    public string? Icon { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
