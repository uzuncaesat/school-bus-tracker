using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Api.Dtos;

/// <summary>API'nin döndürdüğü araç verisi (çıkış).</summary>
public class VehicleDto
{
    public int Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int Capacity { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? ArventoNode { get; set; }  // takip cihazı numarası
    public int AssignedCount { get; set; }   // bu araca atanmış öğrenci sayısı (hesaplanır)
}

/// <summary>Yeni araç eklerken alınan veri (giriş).</summary>
public class VehicleCreateDto
{
    [Required, MaxLength(20)]
    public string Plate { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Driver { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [Range(1, 60)]
    public int Capacity { get; set; } = 12;

    public string Color { get; set; } = "#0e7c7b";

    [MaxLength(40)]
    public string? ArventoNode { get; set; }
}

/// <summary>Var olan aracı güncellerken alınan veri (giriş).</summary>
public class VehicleUpdateDto
{
    [Required, MaxLength(20)] public string Plate { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string Driver { get; set; } = string.Empty;
    public string? Phone { get; set; }
    [Range(1, 60)] public int Capacity { get; set; } = 12;
    public string Color { get; set; } = "#0e7c7b";
    [MaxLength(40)] public string? ArventoNode { get; set; }
}
