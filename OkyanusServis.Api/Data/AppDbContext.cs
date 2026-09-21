using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Data;

/// <summary>
/// EF Core'un veritabanı kapısı. Her DbSet bir tabloyu temsil eder.
/// IdentityDbContext'ten türer → kullanıcı/rol tabloları (AspNetUsers, AspNetRoles...) otomatik gelir.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    // Bağlantı ayarları Program.cs'ten (AddDbContext) buraya enjekte edilir.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Tablolarımız:
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RegistrationDocument> Documents => Set<RegistrationDocument>();
    public DbSet<ServiceConfig> ServiceConfigs => Set<ServiceConfig>();
    public DbSet<ApproachAlert> ApproachAlerts => Set<ApproachAlert>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
}
