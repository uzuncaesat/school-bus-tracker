using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Mappers;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]   // -> /api/notifications
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotificationsController(AppDbContext db) => _db = db;

    // GET /api/notifications?registrationId=5  -> o kaydın bildirimleri (en yeni önce)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetForRegistration([FromQuery] int registrationId)
    {
        var bildirimler = await _db.Notifications
            .Where(n => n.RegistrationId == registrationId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(bildirimler.Select(n => n.ToDto()));
    }

    // GET /api/notifications/mine  -> giriş yapan velinin tüm bildirimleri (en yeni önce)
    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var bildirimler = await _db.Notifications
            .Where(n => n.Registration!.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(bildirimler.Select(n => n.ToDto()));
    }

    // POST /api/notifications/mark-read  -> velinin okunmamış bildirimlerini okundu yap
    [Authorize]
    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkMineRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var okunmamis = await _db.Notifications
            .Where(n => n.Registration!.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var n in okunmamis) n.IsRead = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/notifications  -> yeni bildirim (bindi/indi...) — yönetici/şoför
    [HttpPost]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<NotificationDto>> Create(NotificationCreateDto dto)
    {
        // İş kuralı: bildirim eklenecek kayıt var mı?
        var kayitVar = await _db.Registrations.AnyAsync(r => r.Id == dto.RegistrationId);
        if (!kayitVar)
            return NotFound($"Kayıt bulunamadı: {dto.RegistrationId}");

        var entity = dto.ToEntity();
        _db.Notifications.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForRegistration),
            new { registrationId = entity.RegistrationId }, entity.ToDto());
    }
}
