using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Web.Models;

/// <summary>Yeni araç ekleme formu.</summary>
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

/// <summary>Araç güncelleme (cihaz no dahil).</summary>
public class VehicleUpdateDto
{
    [Required, MaxLength(20)] public string Plate { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string Driver { get; set; } = string.Empty;
    public string? Phone { get; set; }
    [Range(1, 60)] public int Capacity { get; set; } = 12;
    public string Color { get; set; } = "#0e7c7b";
    [MaxLength(40)] public string? ArventoNode { get; set; }
}
