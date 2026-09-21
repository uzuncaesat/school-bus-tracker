namespace OkyanusServis.Api.Entities;

/// <summary>
/// Bir öğrencinin servis kaydı. Veritabanında "Registrations" tablosuna karşılık gelir.
/// </summary>
public class Registration
{
    public int Id { get; set; }                              // Birincil anahtar (PK) — EF otomatik artırır

    // --- Öğrenci ---
    public string StudentName { get; set; } = string.Empty;  // zorunlu (string = boş olamaz)
    public string? StudentTc { get; set; }                   // öğrenci TC kimlik no
    public string School { get; set; } = "Okyanus Koleji";
    public string Grade { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Gidiş-Dönüş"; // Gidiş-Dönüş | Sabah | Akşam

    // --- Veli / iletişim ---
    public string ParentName { get; set; } = string.Empty;
    public string? ParentTc { get; set; }                    // veli TC kimlik no
    public string Phone1 { get; set; } = string.Empty;
    public string? Phone2 { get; set; }                      // opsiyonel (? = boş olabilir)

    // --- Adres / konum ---
    public string Address { get; set; } = string.Empty;
    public string? Area { get; set; }                        // İlçe / mahalle
    public double? Latitude { get; set; }                    // gerçek harita koordinatı (Adım: harita)
    public double? Longitude { get; set; }

    public string? HealthNote { get; set; }                  // alerji, ilaç vb.
    public string? Notes { get; set; }                       // İÇ NOT — sadece personel görür (veliye gösterilmez)

    // --- Operasyon ---
    public string PaymentStatus { get; set; } = "Beklemede"; // manuel takip: Beklemede | Ödendi
    public decimal? Fee { get; set; }                        // toplam anlaşılan ücret (TL)
    public decimal? PaidAmount { get; set; }                 // ödenen tutar (TL)
    public string? PaymentMethod { get; set; }               // (eski/legacy) tek yöntem — yeni yapıda kullanılmıyor
    public string? Bank { get; set; }                        // (eski/legacy) tek banka

    // --- Ödeme planı: peşin + taksit (bölünebilir) ---
    public decimal? DownPayment { get; set; }                // peşin / tek çekim tutarı
    public string? DownPaymentMethod { get; set; }           // Nakit | Elden | Kredi Kartı | Havale/EFT
    public string? DownPaymentBank { get; set; }             // peşin bankası (kart/havale ise)
    public decimal? InstallmentAmount { get; set; }          // taksitli tutar (toplam)
    public string? InstallmentBank { get; set; }             // taksit bankası
    public int? Installments { get; set; }                   // taksit sayısı
    public string? BoardStatus { get; set; }                 // Bindi | İndi | null  (şoför işaretler)
    public bool SkipTomorrow { get; set; }                   // "yarın binmeyecek"
    public bool NotifyOnApproach { get; set; } = true;       // servis yaklaşınca veliye bildirim gitsin mi

    // --- Sözleşme / KVKK ---
    public string? SignedBy { get; set; }                    // dijital imza (ad soyad)
    public bool KvkkAccepted { get; set; }

    // --- İlişkiler ---
    public int? VehicleId { get; set; }                      // FK: hangi araç (opsiyonel)
    public Vehicle? Vehicle { get; set; }                    // navigation: kaydın aracına erişim
    public int? RouteOrder { get; set; }                     // birincil araç güzergahındaki alınış sırası
    public int? SecondVehicleId { get; set; }                // opsiyonel ikinci araç (FK yok, düz kolon)
    public int? SecondRouteOrder { get; set; }               // ikinci araç güzergahındaki sıra
    public int? SiblingOfId { get; set; }                    // kardeş bağlantısı (opsiyonel)
    public string? UserId { get; set; }                      // bu kaydın veli hesabı (AspNetUsers.Id)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
