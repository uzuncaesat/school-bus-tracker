namespace OkyanusServis.Api.Entities;

/// <summary>
/// Servis aracı. "Vehicles" tablosuna karşılık gelir.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }

    public string Plate { get; set; } = string.Empty;   // plaka, örn. "34 SVR 341"
    public string Driver { get; set; } = string.Empty;  // şoför adı
    public string? Phone { get; set; }                  // şoför telefonu
    public int Capacity { get; set; } = 12;             // koltuk kapasitesi
    public string Color { get; set; } = "#0e7c7b";      // haritada/panelde renk

    // Arvento takip cihazı numarası (Node). Bu araç = hangi Arvento cihazı?
    // Yaklaşım motoru konumu bu numaraya göre eşleştirir. Boşsa takip edilmez.
    public string? ArventoNode { get; set; }

    // Bir araca çok kayıt atanır (bir-çok ilişki).
    // Bu koleksiyon "bu aracın öğrencileri" demektir.
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
