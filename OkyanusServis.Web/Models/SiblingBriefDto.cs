namespace OkyanusServis.Web.Models;

/// <summary>Kardeş listesinde gösterilecek kısa kayıt bilgisi.</summary>
public class SiblingBriefDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string? VehiclePlate { get; set; }
}
