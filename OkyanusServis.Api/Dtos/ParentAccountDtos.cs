using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Api.Dtos;

/// <summary>Veli hesabı listeleme öğesi.</summary>
public class ParentAccountDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Students { get; set; } = new();   // bağlı öğrenci adları
}

/// <summary>Şifre sıfırlama.</summary>
public class ResetPasswordDto
{
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
