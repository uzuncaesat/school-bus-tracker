namespace OkyanusServis.Api.Dtos;

/// <summary>Panel özeti — tek çağrıda genel istatistikler.</summary>
public class StatsDto
{
    public int TotalRegistrations { get; set; }
    public int FullService { get; set; }   // Gidiş-Dönüş
    public int Morning { get; set; }        // Sabah
    public int Evening { get; set; }        // Akşam
    public int Paid { get; set; }
    public int Pending { get; set; }
    public int VehicleCount { get; set; }
    public int TotalCapacity { get; set; }
    public int AssignedCount { get; set; }
    public int DocumentCount { get; set; }
    public int ParentAccountCount { get; set; }
    public List<AreaCount> ByArea { get; set; } = new();
}

public class AreaCount
{
    public string Area { get; set; } = string.Empty;
    public int Count { get; set; }
}
