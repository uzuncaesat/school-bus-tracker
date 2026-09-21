using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Web.Models;

/// <summary>Yeni kayıt formunun gönderdiği veri. Backend'deki RegistrationCreateDto ile aynı.</summary>
public class RegistrationCreateDto
{
    [Required, MaxLength(120)]
    public string StudentName { get; set; } = string.Empty;

    [MaxLength(11)] public string? StudentTc { get; set; }
    [MaxLength(11)] public string? ParentTc { get; set; }

    [Required, MaxLength(60)]
    public string Grade { get; set; } = string.Empty;

    public string School { get; set; } = "Okyanus Koleji";

    [Required]
    public string ServiceType { get; set; } = "Gidiş-Dönüş";

    [Required, MaxLength(120)]
    public string ParentName { get; set; } = string.Empty;

    [Required]
    public string Phone1 { get; set; } = string.Empty;
    public string? Phone2 { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string HealthNote { get; set; } = string.Empty;

    public bool KvkkAccepted { get; set; }
    public string? SignedBy { get; set; }

    // Ödeme
    public string PaymentStatus { get; set; } = "Beklemede";
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
}
