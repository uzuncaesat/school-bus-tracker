using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Integrations.Arvento;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/config")]
[Authorize(Roles = "Admin")]   // takip ayarları sadece yönetici
public class ConfigController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IArventoClient _arvento;
    public ConfigController(AppDbContext db, IArventoClient arvento)
    {
        _db = db;
        _arvento = arvento;
    }

    // POST /api/config/test-arvento -> gerçek Arvento bağlantısını dener, tanı metni döner
    [HttpPost("test-arvento")]
    public async Task<ActionResult<string>> TestArvento()
        => Ok(await _arvento.ProbeAsync());

    // GET /api/config -> mevcut takip ayarları
    [HttpGet]
    public async Task<ActionResult<ServiceConfigDto>> Get()
    {
        var c = await _db.ServiceConfigs.FirstOrDefaultAsync();
        if (c is null) return Ok(new ServiceConfigDto());

        return Ok(new ServiceConfigDto
        {
            Enabled = c.Enabled,
            UseMock = c.UseMock,
            ArventoUsername = c.ArventoUsername,
            ArventoPin1 = c.ArventoPin1,
            ArventoPin2 = c.ArventoPin2,
            ArventoBaseUrl = c.ArventoBaseUrl,
            ApproachThresholdMeters = c.ApproachThresholdMeters,
            PollIntervalSeconds = c.PollIntervalSeconds,
            MorningStart = c.MorningStart,
            MorningEnd = c.MorningEnd,
            EveningStart = c.EveningStart,
            EveningEnd = c.EveningEnd,
            VapidReady = !string.IsNullOrEmpty(c.VapidPublicKey)
        });
    }

    // PUT /api/config -> ayarları güncelle
    [HttpPut]
    public async Task<IActionResult> Update(ServiceConfigDto dto)
    {
        var c = await _db.ServiceConfigs.FirstOrDefaultAsync();
        if (c is null) return NotFound();

        c.Enabled = dto.Enabled;
        c.UseMock = dto.UseMock;
        c.ArventoUsername = dto.ArventoUsername;
        c.ArventoPin1 = dto.ArventoPin1;
        c.ArventoPin2 = dto.ArventoPin2;
        c.ArventoBaseUrl = dto.ArventoBaseUrl;
        c.ApproachThresholdMeters = dto.ApproachThresholdMeters;
        c.PollIntervalSeconds = dto.PollIntervalSeconds;
        c.MorningStart = dto.MorningStart;
        c.MorningEnd = dto.MorningEnd;
        c.EveningStart = dto.EveningStart;
        c.EveningEnd = dto.EveningEnd;
        c.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
