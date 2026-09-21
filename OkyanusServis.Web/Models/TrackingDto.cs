namespace OkyanusServis.Web.Models;

/// <summary>Velinin aracının canlı konum bilgisi (servis saatlerinde).</summary>
public class TrackingDto
{
    public bool Active { get; set; }
    public string? Reason { get; set; }
    public string? Leg { get; set; }
    public List<BusDto> Buses { get; set; } = new();
    public double? HomeLat { get; set; }
    public double? HomeLng { get; set; }
}

public class BusDto
{
    public string Plate { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double Speed { get; set; }
}
