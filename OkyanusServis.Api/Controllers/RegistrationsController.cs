using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Mappers;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]   // -> /api/registrations
public class RegistrationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _users;

    public RegistrationsController(AppDbContext db, UserManager<IdentityUser> users)
    {
        _db = db;
        _users = users;
    }

    // GET /api/registrations/mine -> giriş yapan velinin kendi kayıtları
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RegistrationDto>>> Mine()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var list = await _db.Registrations
            .Include(r => r.Vehicle)
            .Where(r => r.UserId == uid)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        // İç notlar veliye gösterilmez
        return Ok(list.Select(r => { var d = r.ToDto(); d.Notes = null; return d; }));
    }

    // POST /api/registrations/5/parent-account -> kayda veli girişi tanımla (sadece Admin)
    // E-posta zaten varsa (kardeş) o hesaba bağlar, yoksa yeni hesap açar.
    [HttpPost("{id:int}/parent-account")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateParentAccount(int id, ParentAccountCreateDto dto)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg is null) return NotFound();

        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest("Yeni veli hesabı için en az 6 karakterli şifre gerekli.");

            user = new IdentityUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
            var res = await _users.CreateAsync(user, dto.Password);
            if (!res.Succeeded)
                return BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));
        }
        if (!await _users.IsInRoleAsync(user, "Veli"))
            await _users.AddToRoleAsync(user, "Veli");

        reg.UserId = user.Id;
        await _db.SaveChangesAsync();

        // Kardeşlere de aynı veli hesabını uygula (tek girişle hepsini görsün)
        await PropagateUserIdAsync(reg);
        await _db.SaveChangesAsync();
        return Ok(new { email = user.Email });
    }

    // ---- Güzergah sırası ----
    public record RouteOrderDto(int VehicleId, List<int> OrderedIds);

    // POST /api/registrations/route-order -> bir aracın öğrenci alınış sırasını topluca kaydeder (Admin)
    [HttpPost("route-order")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<IActionResult> SetRouteOrder(RouteOrderDto dto)
    {
        var regs = await _db.Registrations
            .Where(r => r.VehicleId == dto.VehicleId || r.SecondVehicleId == dto.VehicleId)
            .ToListAsync();

        for (int i = 0; i < dto.OrderedIds.Count; i++)
        {
            var reg = regs.FirstOrDefault(r => r.Id == dto.OrderedIds[i]);
            if (reg is null) continue;
            // Bu araç öğrencinin birincil aracı mı, ikinci aracı mı? Doğru sıra alanını yaz.
            if (reg.VehicleId == dto.VehicleId) reg.RouteOrder = i + 1;
            else reg.SecondRouteOrder = i + 1;
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---- Kardeş / aile ----

    public record SiblingBriefDto(int Id, string StudentName, string Grade, string? VehiclePlate);

    private async Task<List<Entities.Registration>> FamilyMembersAsync(Entities.Registration reg)
    {
        var primaryId = reg.SiblingOfId ?? reg.Id;
        return await _db.Registrations
            .Include(r => r.Vehicle)
            .Where(r => r.Id == primaryId || r.SiblingOfId == primaryId)
            .ToListAsync();
    }

    // Ailedeki bir veli hesabını (varsa) hesabı olmayan kardeşlere kopyalar.
    private async Task PropagateUserIdAsync(Entities.Registration reg)
    {
        var family = await FamilyMembersAsync(reg);
        var userId = family.Select(f => f.UserId).FirstOrDefault(u => u != null);
        if (userId is null) return;
        foreach (var m in family)
            if (m.UserId is null) m.UserId = userId;
    }

    // GET /api/registrations/5/siblings -> bu kaydın kardeşleri (kendisi hariç)
    [HttpGet("{id:int}/siblings")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<IEnumerable<SiblingBriefDto>>> GetSiblings(int id)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg is null) return NotFound();

        var family = await FamilyMembersAsync(reg);
        return Ok(family.Where(m => m.Id != id)
            .Select(m => new SiblingBriefDto(m.Id, m.StudentName, m.Grade, m.Vehicle?.Plate)));
    }

    // POST /api/registrations/5/sibling/8 -> 5'i, 8'in ailesine kardeş olarak bağla (Admin)
    [HttpPost("{id:int}/sibling/{targetId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LinkSibling(int id, int targetId)
    {
        if (id == targetId) return BadRequest("Bir kayıt kendisiyle kardeş olamaz.");
        var reg = await _db.Registrations.FindAsync(id);
        var target = await _db.Registrations.FindAsync(targetId);
        if (reg is null || target is null) return NotFound();

        var targetPrimary = target.SiblingOfId ?? target.Id;
        var regPrimary = reg.SiblingOfId ?? reg.Id;
        if (regPrimary == targetPrimary) return NoContent(); // zaten aynı aile

        // reg'in mevcut ailesindeki herkesi hedef ailenin birincil kaydına bağla
        var regFamily = await _db.Registrations
            .Where(r => r.Id == regPrimary || r.SiblingOfId == regPrimary)
            .ToListAsync();
        foreach (var m in regFamily)
            m.SiblingOfId = (m.Id == targetPrimary) ? null : targetPrimary;

        await _db.SaveChangesAsync();
        await PropagateUserIdAsync(target);   // hesabı aile geneline yay
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/registrations/5/sibling -> 5'i aileden ayır (Admin)
    [HttpDelete("{id:int}/sibling")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnlinkSibling(int id)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg is null) return NotFound();

        // Eğer bu kayıt ailenin birincili ise, ona bağlı kardeşleri de serbest bırak
        var children = await _db.Registrations.Where(r => r.SiblingOfId == id).ToListAsync();
        foreach (var c in children) c.SiblingOfId = null;
        reg.SiblingOfId = null;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/registrations  -> tüm kayıtlar (en yeni önce) — sadece yönetici/şoför
    [HttpGet]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<IEnumerable<RegistrationDto>>> GetAll()
    {
        var kayitlar = await _db.Registrations
            .Include(r => r.Vehicle)                 // aracını da getir (plaka için)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(kayitlar.Select(r => r.ToDto())); // entity -> dto
    }

    // GET /api/registrations/5  -> tek kayıt (personel; veli /mine kullanır)
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<RegistrationDto>> GetById(int id)
    {
        var kayit = await _db.Registrations
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (kayit is null)
            return NotFound();                       // 404

        return Ok(kayit.ToDto());
    }

    // POST /api/registrations  -> yeni kayıt
    [HttpPost]
    public async Task<ActionResult<RegistrationDto>> Create(RegistrationCreateDto dto)
    {
        // [ApiController] sayesinde dto geçersizse buraya hiç gelmeden 400 döner.
        var entity = dto.ToEntity();

        _db.Registrations.Add(entity);
        await _db.SaveChangesAsync();                // INSERT çalışır, Id atanır

        // 201 Created + yeni kaydın adresini (Location header) döndürür
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    // PUT /api/registrations/5  -> güncelle (araç atama, ödeme/durum vb.) — yönetici/şoför
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<RegistrationDto>> Update(int id, RegistrationUpdateDto dto)
    {
        var kayit = await _db.Registrations.FindAsync(id);
        if (kayit is null)
            return NotFound();

        kayit.ApplyUpdate(dto);                      // sadece dolu alanları uygula
        await _db.SaveChangesAsync();                // UPDATE çalışır

        // Yanıtta güncel plaka da olsun diye aracı yükle
        await _db.Entry(kayit).Reference(r => r.Vehicle).LoadAsync();
        return Ok(kayit.ToDto());
    }

    // PUT /api/registrations/5/details -> temel bilgileri düzenle (öğrenci/veli/adres)
    [HttpPut("{id:int}/details")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult<RegistrationDto>> UpdateDetails(int id, RegistrationEditDto dto)
    {
        var kayit = await _db.Registrations.FindAsync(id);
        if (kayit is null) return NotFound();

        kayit.ApplyEdit(dto);
        await _db.SaveChangesAsync();

        await _db.Entry(kayit).Reference(r => r.Vehicle).LoadAsync();
        return Ok(kayit.ToDto());
    }

    // DELETE /api/registrations/5  -> sil — sadece yönetici
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var kayit = await _db.Registrations.FindAsync(id);
        if (kayit is null)
            return NotFound();

        _db.Registrations.Remove(kayit);
        await _db.SaveChangesAsync();                // DELETE çalışır
        return NoContent();                          // 204
    }
}
