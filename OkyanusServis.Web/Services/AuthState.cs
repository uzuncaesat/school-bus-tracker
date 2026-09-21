using System.Security.Claims;

namespace OkyanusServis.Web.Services;

/// <summary>Geçerli oturumun token'ını ve kullanıcısını bellekte tutan basit taşıyıcı.</summary>
public class AuthState
{
    public string? Token { get; set; }
    public ClaimsPrincipal User { get; set; } = new(new ClaimsIdentity());
}
