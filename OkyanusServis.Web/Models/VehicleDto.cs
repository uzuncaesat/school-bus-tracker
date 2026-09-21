namespace OkyanusServis.Web.Models;

/// <summary>API'den gelen araç verisi.</summary>
public class VehicleDto
{
    public int Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int Capacity { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? ArventoNode { get; set; }
    public int AssignedCount { get; set; }
}
