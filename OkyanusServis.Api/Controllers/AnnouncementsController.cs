using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Mappers;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]   // -> /api/announcements
public class AnnouncementsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AnnouncementsController(AppDbContext db) => _db = db;

    // GET /api/announcements  -> en yeni önce
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAll()
    {
        var duyurular = await _db.Announcements
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(duyurular.Select(a => a.ToDto()));
    }

    // POST /api/announcements  -> tüm velilere duyuru — sadece yönetici
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AnnouncementDto>> Create(AnnouncementCreateDto dto)
    {
        var entity = dto.ToEntity();
        _db.Announcements.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, entity.ToDto());
    }
}
