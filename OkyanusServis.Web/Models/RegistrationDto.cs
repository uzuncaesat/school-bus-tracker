namespace OkyanusServis.Web.Models;

/// <summary>API'den gelen kayıt verisi. Backend'deki RegistrationDto ile aynı alanlar.</summary>
public class RegistrationDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentTc { get; set; }
    public string? ParentTc { get; set; }
    public string School { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string Phone1 { get; set; } = string.Empty;
    public string? Phone2 { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Area { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? HealthNote { get; set; }
    public string? Notes { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal? Fee { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Bank { get; set; }
    public decimal? DownPayment { get; set; }
    public string? DownPaymentMethod { get; set; }
    public string? DownPaymentBank { get; set; }
    public decimal? InstallmentAmount { get; set; }
    public string? InstallmentBank { get; set; }
    public int? Installments { get; set; }
    public string? BoardStatus { get; set; }
    public bool SkipTomorrow { get; set; }
    public int? VehicleId { get; set; }
    public string? VehiclePlate { get; set; }
    public int? RouteOrder { get; set; }
    public int? SecondVehicleId { get; set; }
    public int? SecondRouteOrder { get; set; }
    public int? SiblingOfId { get; set; }
    public bool HasParentAccount { get; set; }
    public DateTime CreatedAt { get; set; }
}
