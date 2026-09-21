using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]   // -> /api/stats
[Authorize(Roles = "Admin,Driver")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<StatsDto>> Get()
    {
        var regs = await _db.Registrations.ToListAsync();
        var vehicles = await _db.Vehicles.ToListAsync();
        var docCount = await _db.Documents.CountAsync();

        var dto = new StatsDto
        {
            TotalRegistrations = regs.Count,
            FullService = regs.Count(r => r.ServiceType == "Gidiş-Dönüş"),
            Morning = regs.Count(r => r.ServiceType == "Sabah"),
            Evening = regs.Count(r => r.ServiceType == "Akşam"),
            Paid = regs.Count(r => r.PaymentStatus == "Ödendi"),
            Pending = regs.Count(r => r.PaymentStatus != "Ödendi"),
            VehicleCount = vehicles.Count,
            TotalCapacity = vehicles.Sum(v => v.Capacity),
            AssignedCount = regs.Count(r => r.VehicleId != null),
            DocumentCount = docCount,
            ParentAccountCount = regs.Count(r => r.UserId != null),
            ByArea = regs.Where(r => !string.IsNullOrEmpty(r.Area))
                         .GroupBy(r => r.Area!)
                         .Select(g => new AreaCount { Area = g.Key, Count = g.Count() })
                         .OrderByDescending(a => a.Count)
                         .ToList()
        };
        return Ok(dto);
    }
}
