using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Api.Dtos;

/// <summary>Personel hesabı listeleme öğesi.</summary>
public class StaffAccountDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // Admin | Driver
}

/// <summary>Yeni personel hesabı oluşturma.</summary>
public class StaffCreateDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Admin";   // Admin (yönetici/ofis) | Driver (şoför)
}
