using System.ComponentModel.DataAnnotations;

namespace OkyanusServis.Api.Dtos;

/// <summary>API'nin DIŞARIYA döndürdüğü kayıt verisi (çıkış).</summary>
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
    public string? Notes { get; set; }   // iç not (personel); veli çağrısında boşaltılır

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
    public string? VehiclePlate { get; set; }   // Vehicle.Plate'i düzleştirdik (kolaylık)
    public int? RouteOrder { get; set; }         // güzergahta alınış sırası
    public int? SecondVehicleId { get; set; }    // opsiyonel ikinci araç
    public int? SecondRouteOrder { get; set; }
    public int? SiblingOfId { get; set; }
    public bool HasParentAccount { get; set; }   // bu kayda veli girişi tanımlı mı
    public DateTime CreatedAt { get; set; }
}

/// <summary>Var olan kaydın temel bilgilerini (öğrenci/veli/adres) düzenleme.</summary>
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
    public string? Area { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? HealthNote { get; set; }
}

/// <summary>Bir kayda veli girişi (kullanıcı/şifre) oluşturma.</summary>
public class ParentAccountCreateDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Yeni hesap için gerekli; e-posta zaten varsa (kardeş) yok sayılır.
    public string? Password { get; set; }
}

/// <summary>Yeni kayıt oluştururken DIŞARIDAN alınan veri (giriş).
/// Dikkat: Id, CreatedAt, PaymentStatus, VehicleId burada YOK — onları sunucu yönetir.</summary>
public class RegistrationCreateDto
{
    [Required, MaxLength(120)]
    public string StudentName { get; set; } = string.Empty;

    [MaxLength(11)] public string? StudentTc { get; set; }
    [MaxLength(11)] public string? ParentTc { get; set; }

    [Required, MaxLength(60)]
    public string Grade { get; set; } = string.Empty;

    [MaxLength(120)]
    public string School { get; set; } = "Okyanus Koleji";

    [Required]
    public string ServiceType { get; set; } = "Gidiş-Dönüş";  // Gidiş-Dönüş | Sabah | Akşam

    [Required, MaxLength(120)]
    public string ParentName { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone1 { get; set; } = string.Empty;
    public string? Phone2 { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;
    public string? Area { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? HealthNote { get; set; }

    public bool KvkkAccepted { get; set; }
    public string? SignedBy { get; set; }

    // Ödeme (kayıt alırken tek adımda)
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

/// <summary>Var olan kaydı güncellerken alınan veri (araç atama, ödeme/durum güncelleme vb.).</summary>
public class RegistrationUpdateDto
{
    public string? PaymentStatus { get; set; }   // Beklemede | Ödendi
    public decimal? Fee { get; set; }             // toplam ücret
    public decimal? PaidAmount { get; set; }      // ödenen tutar
    public string? PaymentMethod { get; set; }    // (legacy)
    public string? Bank { get; set; }             // (legacy)
    public decimal? DownPayment { get; set; }     // peşin/tek çekim tutarı
    public string? DownPaymentMethod { get; set; }// peşin yöntemi
    public string? DownPaymentBank { get; set; }  // peşin bankası
    public decimal? InstallmentAmount { get; set; }// taksitli tutar
    public string? InstallmentBank { get; set; }  // taksit bankası
    public int? Installments { get; set; }        // taksit sayısı
    public string? Notes { get; set; }            // iç not
    public string? BoardStatus { get; set; }      // Bindi | İndi
    public bool? SkipTomorrow { get; set; }
    public int? VehicleId { get; set; }
    public int? SecondVehicleId { get; set; }      // ikinci araç (null = temizle)
}
