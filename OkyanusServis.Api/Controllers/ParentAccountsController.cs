using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;
using OkyanusServis.Api.Dtos;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/parent-accounts")]
[Authorize(Roles = "Admin")]
public class ParentAccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _users;

    public ParentAccountsController(AppDbContext db, UserManager<IdentityUser> users)
    {
        _db = db;
        _users = users;
    }

    // GET /api/parent-accounts -> tüm veli hesapları + bağlı öğrenciler
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParentAccountDto>>> List()
    {
        var veliUsers = await _users.GetUsersInRoleAsync("Veli");
        var ids = veliUsers.Select(u => u.Id).ToList();

        var regs = await _db.Registrations
            .Where(r => r.UserId != null && ids.Contains(r.UserId))
            .Select(r => new { r.UserId, r.StudentName })
            .ToListAsync();

        var list = veliUsers.Select(u => new ParentAccountDto
        {
            UserId = u.Id,
            Email = u.Email ?? u.UserName ?? "",
            Students = regs.Where(x => x.UserId == u.Id).Select(x => x.StudentName).ToList()
        })
        .OrderBy(a => a.Email)
        .ToList();

        return Ok(list);
    }

    // POST /api/parent-accounts/{userId}/reset-password -> şifre sıfırla
    [HttpPost("{userId}/reset-password")]
    public async Task<IActionResult> ResetPassword(string userId, ResetPasswordDto dto)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var res = await _users.ResetPasswordAsync(user, token, dto.Password);
        if (!res.Succeeded)
            return BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));

        return Ok();
    }
}
