using Microsoft.AspNetCore.Identity;

namespace OkyanusServis.Api.Data;

/// <summary>Roller (Admin, Driver) ve başlangıç yönetici hesabını oluşturur.</summary>
public static class IdentitySeeder
{
    public const string AdminEmail = "admin@okyanusservis.com";
    public const string AdminPassword = "Admin!123";

    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = sp.GetRequiredService<UserManager<IdentityUser>>();

        // Roller
        foreach (var role in new[] { "Admin", "Driver", "Veli" })
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));

        // Başlangıç yönetici
        var admin = await userMgr.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = AdminEmail, Email = AdminEmail, EmailConfirmed = true };
            await userMgr.CreateAsync(admin, AdminPassword);
            await userMgr.AddToRoleAsync(admin, "Admin");
        }
    }
}
