using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]   // belgeler kişisel veri → giriş şart
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DocumentsController(AppDbContext db) => _db = db;

    // Personel her kayda erişir; veli yalnızca KENDİ kaydına.
    private async Task<bool> CanAccess(int registrationId)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Driver")) return true;
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _db.Registrations.AnyAsync(r => r.Id == registrationId && r.UserId == uid);
    }

    private const long MaxSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] Allowed =
    {
        "application/pdf",
        "image/jpeg", "image/png",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    // GET /api/registrations/5/documents -> o kaydın belgeleri (içerik hariç)
    [HttpGet("registrations/{registrationId:int}/documents")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> List(int registrationId)
    {
        if (!await CanAccess(registrationId)) return Forbid();

        var docs = await _db.Documents
            .Where(d => d.RegistrationId == registrationId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                RegistrationId = d.RegistrationId,
                FileName = d.FileName,
                ContentType = d.ContentType,
                Size = d.Size,
                UploadedAt = d.UploadedAt
            })
            .ToListAsync();
        return Ok(docs);
    }

    // POST /api/registrations/5/documents -> belge yükle (multipart)
    [HttpPost("registrations/{registrationId:int}/documents")]
    [RequestSizeLimit(MaxSize + 1024 * 1024)]
    public async Task<ActionResult<DocumentDto>> Upload(int registrationId, IFormFile file)
    {
        if (!await _db.Registrations.AnyAsync(r => r.Id == registrationId))
            return NotFound($"Kayıt bulunamadı: {registrationId}");
        if (file is null || file.Length == 0)
            return BadRequest("Dosya boş.");
        if (file.Length > MaxSize)
            return BadRequest("Dosya 10 MB'tan büyük olamaz.");
        if (!Allowed.Contains(file.ContentType))
            return BadRequest("Desteklenmeyen tür. PDF, Word, JPG veya PNG yükleyin.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var doc = new RegistrationDocument
        {
            RegistrationId = registrationId,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Size = file.Length,
            Data = ms.ToArray()
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        return Ok(new DocumentDto
        {
            Id = doc.Id,
            RegistrationId = doc.RegistrationId,
            FileName = doc.FileName,
            ContentType = doc.ContentType,
            Size = doc.Size,
            UploadedAt = doc.UploadedAt
        });
    }

    // GET /api/documents/5 -> belgeyi indir (kendi kaydı ise veli de erişir)
    [HttpGet("documents/{id:int}")]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc is null) return NotFound();
        if (!await CanAccess(doc.RegistrationId)) return Forbid();
        return File(doc.Data, doc.ContentType, doc.FileName);
    }

    // DELETE /api/documents/5 -> belge sil (sadece Admin)
    [HttpDelete("documents/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc is null) return NotFound();
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
