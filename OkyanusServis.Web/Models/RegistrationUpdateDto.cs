namespace OkyanusServis.Web.Models;

/// <summary>Kayıt güncelleme (araç atama, ödeme/durum). Null gönderilen alan değişmez.</summary>
public class RegistrationUpdateDto
{
    public string? PaymentStatus { get; set; }   // Beklemede | Ödendi
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
    public string? Notes { get; set; }
    public string? BoardStatus { get; set; }      // Bindi | İndi
    public bool? SkipTomorrow { get; set; }
    public int? VehicleId { get; set; }
    public int? SecondVehicleId { get; set; }
}
