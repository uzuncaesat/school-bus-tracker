namespace OkyanusServis.Web.Models;

public class StatsDto
{
    public int TotalRegistrations { get; set; }
    public int FullService { get; set; }
    public int Morning { get; set; }
    public int Evening { get; set; }
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
