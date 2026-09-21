using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Entities;
using OkyanusServis.Api.Integrations.Arvento;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/tracking")]
public class TrackingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IArventoClient _arvento;

    public TrackingController(AppDbContext db, IArventoClient arvento)
    {
        _db = db;
        _arvento = arvento;
    }

    public record BusDto(string Plate, double Lat, double Lng, double Speed);
    public record TrackingDto(bool Active, string? Reason, string? Leg,
                              List<BusDto> Buses, double? HomeLat, double? HomeLng);

    /// <summary>Giriş yapan velinin aracının canlı konumu — SADECE servis saatlerinde.</summary>
    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<TrackingDto>> Mine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // Velinin kayıtları (araç + koordinat)
        var regs = await _db.Registrations
            .Include(r => r.Vehicle)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        // Evi (ilk koordinatlı kayıt) haritayı ortalamak için
        var home = regs.FirstOrDefault(r => r.Latitude != null && r.Longitude != null);
        double? homeLat = home?.Latitude;
        double? homeLng = home?.Longitude;

        var cfg = await _db.ServiceConfigs.AsNoTracking().FirstOrDefaultAsync() ?? new ServiceConfig();

        if (!cfg.Enabled)
            return Ok(new TrackingDto(false, "Takip şu an kapalı.", null, new(), homeLat, homeLng));

        var leg = ServiceHours.CurrentLeg(cfg, DateTime.Now);
        if (leg is null)
            return Ok(new TrackingDto(false, "Servis şu an aktif değil (servis saatleri dışında).", null, new(), homeLat, homeLng));

        // Velinin araçlarının Arvento cihaz numaraları
        var myNodes = regs
            .Where(r => r.Vehicle?.ArventoNode != null && r.Vehicle.ArventoNode != "")
            .Select(r => new { r.Vehicle!.ArventoNode, r.Vehicle.Plate })
            .Distinct()
            .ToList();

        if (myNodes.Count == 0)
            return Ok(new TrackingDto(false, "Aracınız henüz takip sistemine tanımlı değil.", leg, new(), homeLat, homeLng));

        var positions = (await _arvento.GetPositionsAsync())
            .GroupBy(p => p.Node)
            .ToDictionary(g => g.Key, g => g.First());

        var buses = new List<BusDto>();
        foreach (var n in myNodes)
            if (positions.TryGetValue(n.ArventoNode!, out var pos))
                buses.Add(new BusDto(n.Plate, pos.Latitude, pos.Longitude, pos.Speed));

        if (buses.Count == 0)
            return Ok(new TrackingDto(false, "Araç konumu şu an alınamıyor.", leg, new(), homeLat, homeLng));

        return Ok(new TrackingDto(true, null, leg, buses, homeLat, homeLng));
    }
}
