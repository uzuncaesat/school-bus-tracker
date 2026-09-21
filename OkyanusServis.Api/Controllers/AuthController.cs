using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // GET /api/auth/me -> giriş yapan kullanıcının email + rolleri
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var email = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        return Ok(new { email, roles });
    }
}
