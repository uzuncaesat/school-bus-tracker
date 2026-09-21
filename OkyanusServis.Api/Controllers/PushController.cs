using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Entities;
using OkyanusServis.Api.Integrations.Arvento;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/push")]
public class PushController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly WebPushSender _sender;

    public PushController(AppDbContext db, WebPushSender sender)
    {
        _db = db;
        _sender = sender;
    }

    /// <summary>Tarayıcının aboneliği kurarken kullanacağı VAPID public anahtarı.</summary>
    [HttpGet("vapid-public-key")]
    public async Task<ActionResult<string>> GetVapidPublicKey()
    {
        var cfg = await _db.ServiceConfigs.AsNoTracking().FirstOrDefaultAsync();
        return Ok(cfg?.VapidPublicKey ?? "");
    }

    public record SubscribeDto(string Endpoint, string P256dh, string Auth);

    /// <summary>Giriş yapmış velinin bu cihaz için push aboneliğini kaydeder (upsert).</summary>
    [Authorize]
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Endpoint)) return BadRequest("Endpoint boş.");

        var existing = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == dto.Endpoint);
        if (existing is null)
        {
            _db.PushSubscriptions.Add(new Entities.PushSubscription
            {
                UserId = userId,
                Endpoint = dto.Endpoint,
                P256dh = dto.P256dh,
                Auth = dto.Auth
            });
        }
        else
        {
            existing.UserId = userId;      // cihaz sahibi değiştiyse güncelle
            existing.P256dh = dto.P256dh;
            existing.Auth = dto.Auth;
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Bu cihazın aboneliğini kaldırır.</summary>
    [Authorize]
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] SubscribeDto dto)
    {
        var subs = await _db.PushSubscriptions.Where(s => s.Endpoint == dto.Endpoint).ToListAsync();
        if (subs.Count > 0)
        {
            _db.PushSubscriptions.RemoveRange(subs);
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    /// <summary>Kendine deneme bildirimi gönderir — telefona düşüyor mu diye test.</summary>
    [Authorize]
    [HttpPost("test")]
    public async Task<IActionResult> Test()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        await _sender.SendToUserAsync(userId, "Test bildirimi 🚌", "Bildirimler çalışıyor! Servis yaklaşınca böyle haber vereceğiz.");
        return NoContent();
    }
}
