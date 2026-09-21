using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OkyanusServis.Api.Dtos;

namespace OkyanusServis.Api.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    public StaffController(UserManager<IdentityUser> users) => _users = users;

    private static readonly string[] ValidRoles = { "Admin", "Driver" };

    // GET /api/staff -> yönetici + şoför hesapları
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffAccountDto>>> List()
    {
        var admins = await _users.GetUsersInRoleAsync("Admin");
        var drivers = await _users.GetUsersInRoleAsync("Driver");

        var list = new List<StaffAccountDto>();
        foreach (var u in admins)
            list.Add(new StaffAccountDto { UserId = u.Id, Email = u.Email ?? u.UserName ?? "", Role = "Admin" });
        foreach (var u in drivers)
            if (!list.Any(x => x.UserId == u.Id))   // aynı kullanıcı iki rolde olabilir; Admin öncelikli
                list.Add(new StaffAccountDto { UserId = u.Id, Email = u.Email ?? u.UserName ?? "", Role = "Driver" });

        return Ok(list.OrderBy(x => x.Email));
    }

    // POST /api/staff -> yeni personel hesabı
    [HttpPost]
    public async Task<IActionResult> Create(StaffCreateDto dto)
    {
        if (!ValidRoles.Contains(dto.Role))
            return BadRequest("Geçersiz rol. Admin veya Driver olmalı.");
        if (await _users.FindByEmailAsync(dto.Email) is not null)
            return BadRequest("Bu e-posta zaten kullanılıyor.");

        var user = new IdentityUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
        var res = await _users.CreateAsync(user, dto.Password);
        if (!res.Succeeded)
            return BadRequest(string.Join("; ", res.Errors.Select(e => e.Description)));

        await _users.AddToRoleAsync(user, dto.Role);
        return Ok(new StaffAccountDto { UserId = user.Id, Email = user.Email!, Role = dto.Role });
    }

    // POST /api/staff/{userId}/reset-password
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

    // DELETE /api/staff/{userId} -> personel sil (kendini silemez)
    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == currentId)
            return BadRequest("Kendi hesabınızı silemezsiniz.");

        var user = await _users.FindByIdAsync(userId);
        if (user is null) return NotFound();
        await _users.DeleteAsync(user);
        return NoContent();
    }
}
