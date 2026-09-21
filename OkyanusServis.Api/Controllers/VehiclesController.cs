using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Entities;
using OkyanusServis.Api.Integrations.Arvento;
using OkyanusServis.Api.Mappers;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]   // -> /api/vehicles
public class VehiclesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IArventoClient _arvento;
    public VehiclesController(AppDbContext db, IArventoClient arvento)
    {
        _db = db;
        _arvento = arvento;
    }

    // POST /api/vehicles/import-arvento -> Arvento'daki araçları (plaka+cihaz no) içe aktarır (Admin)
    [HttpPost("import-arvento")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> ImportFromArvento()
    {
        var nodes = await _arvento.GetNodesAsync();
        if (nodes.Count == 0)
            return Ok(new { created = 0, updated = 0, total = 0, message = "Arvento'dan araç gelmedi (bağlantı/yetki?)." });

        var vehicles = await _db.Vehicles.ToListAsync();
        int created = 0, updated = 0;

        bool Junk(string? p) => string.IsNullOrWhiteSpace(p)
            || p!.Trim().ToLower() is "boş" or "boşş" or "bos" or "-" or ".";

        foreach (var (node, plate) in nodes)
        {
            if (string.IsNullOrWhiteSpace(node)) continue;
            var cleanPlate = plate?.Trim();

            var byNode = vehicles.FirstOrDefault(v => v.ArventoNode == node);
            if (byNode is not null)
            {
                if (!Junk(cleanPlate) && byNode.Plate != cleanPlate) { byNode.Plate = cleanPlate!; updated++; }
                continue;
            }
            var byPlate = Junk(cleanPlate) ? null : vehicles.FirstOrDefault(v => v.Plate == cleanPlate);
            if (byPlate is not null)
            {
                byPlate.ArventoNode = node; updated++;
                continue;
            }
            var yeni = new Vehicle
            {
                Plate = Junk(cleanPlate) ? node : cleanPlate!,
                ArventoNode = node,
                Driver = "",
                Capacity = 12,
                Color = "#0e7c7b"
            };
            _db.Vehicles.Add(yeni);
            vehicles.Add(yeni);
            created++;
        }

        await _db.SaveChangesAsync();
        return Ok(new { created, updated, total = nodes.Count });
    }

    // GET /api/vehicles  -> araçlar + atanmış öğrenci sayısı
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
    {
        var araclar = await _db.Vehicles
            .Include(v => v.Registrations)   // AssignedCount hesaplayabilmek için
            .ToListAsync();

        return Ok(araclar.Select(v => v.ToDto()));
    }

    // GET /api/vehicles/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDto>> GetById(int id)
    {
        var arac = await _db.Vehicles
            .Include(v => v.Registrations)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (arac is null) return NotFound();
        return Ok(arac.ToDto());
    }

    // POST /api/vehicles — sadece yönetici
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehicleDto>> Create(VehicleCreateDto dto)
    {
        var entity = dto.ToEntity();
        _db.Vehicles.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    // PUT /api/vehicles/5 — araç bilgilerini güncelle (cihaz no dahil) — yönetici
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, VehicleUpdateDto dto)
    {
        var arac = await _db.Vehicles.FindAsync(id);
        if (arac is null) return NotFound();

        arac.ApplyUpdate(dto);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/vehicles/5 — sadece yönetici
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var arac = await _db.Vehicles.FindAsync(id);
        if (arac is null) return NotFound();

        _db.Vehicles.Remove(arac);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
