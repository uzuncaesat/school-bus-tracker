namespace OkyanusServis.Api.Entities;

/// <summary>
/// Bir öğrenciye, belirli bir gün ve sefer için "servis yaklaşıyor" bildiriminin
/// gittiğini kaydeder. Aynı sefer içinde tekrar bildirim gitmesini engeller.
/// "ApproachAlerts" tablosuna karşılık gelir.
/// </summary>
public class ApproachAlert
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }          // hangi öğrenci
    public Registration? Registration { get; set; }

    public int VehicleId { get; set; }               // hangi araç yaklaştı
    public DateOnly Date { get; set; }               // hangi gün
    public string Leg { get; set; } = "Sabah";       // "Sabah" | "Akşam" (sefer yönü)

    public double DistanceMeters { get; set; }       // bildirim anındaki mesafe
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
