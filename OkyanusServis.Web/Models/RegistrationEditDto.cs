using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Web.Models;

/// <summary>Var olan kaydın temel bilgilerini düzenleme formu.</summary>
public class RegistrationEditDto
{
    [Required, MaxLength(120)] public string StudentName { get; set; } = string.Empty;
    [MaxLength(11)] public string? StudentTc { get; set; }
    [MaxLength(11)] public string? ParentTc { get; set; }
    [MaxLength(120)] public string School { get; set; } = "Okyanus Koleji";
    [Required, MaxLength(60)] public string Grade { get; set; } = string.Empty;
    [Required] public string ServiceType { get; set; } = "Gidiş-Dönüş";
    [Required, MaxLength(120)] public string ParentName { get; set; } = string.Empty;
    [Required] public string Phone1 { get; set; } = string.Empty;
    public string? Phone2 { get; set; }
    [Required] public string Address { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string HealthNote { get; set; } = string.Empty;
}
