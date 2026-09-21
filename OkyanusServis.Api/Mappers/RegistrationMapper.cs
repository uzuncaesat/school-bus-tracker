using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Mappers;

/// <summary>Registration entity'si ile DTO'ları arasında elle dönüşümler.</summary>
public static class RegistrationMapper
{
    /// <summary>Entity -> Dto (dışarıya döndürmek için).</summary>
    public static RegistrationDto ToDto(this Registration r) => new()
    {
        Id = r.Id,
        StudentName = r.StudentName,
        StudentTc = r.StudentTc,
        ParentTc = r.ParentTc,
        School = r.School,
        Grade = r.Grade,
        ServiceType = r.ServiceType,
        ParentName = r.ParentName,
        Phone1 = r.Phone1,
        Phone2 = r.Phone2,
        Address = r.Address,
        Area = r.Area,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        HealthNote = r.HealthNote,
        Notes = r.Notes,
        PaymentStatus = r.PaymentStatus,
        Fee = r.Fee,
        PaidAmount = r.PaidAmount,
        PaymentMethod = r.PaymentMethod,
        Bank = r.Bank,
        DownPayment = r.DownPayment,
        DownPaymentMethod = r.DownPaymentMethod,
        DownPaymentBank = r.DownPaymentBank,
        InstallmentAmount = r.InstallmentAmount,
        InstallmentBank = r.InstallmentBank,
        Installments = r.Installments,
        BoardStatus = r.BoardStatus,
        SkipTomorrow = r.SkipTomorrow,
        VehicleId = r.VehicleId,
        VehiclePlate = r.Vehicle?.Plate,   // ilişkili araçtan plakayı düzleştir (null olabilir)
        RouteOrder = r.RouteOrder,
        SecondVehicleId = r.SecondVehicleId,
        SecondRouteOrder = r.SecondRouteOrder,
        SiblingOfId = r.SiblingOfId,
        HasParentAccount = r.UserId != null,
        CreatedAt = r.CreatedAt,
    };

    /// <summary>CreateDto -> Entity (yeni kayıt oluştururken).
    /// Id, CreatedAt, PaymentStatus gibi alanları BİLEREK atamıyoruz — onları sunucu yönetir.</summary>
    public static Registration ToEntity(this RegistrationCreateDto d) => new()
    {
        StudentName = d.StudentName,
        StudentTc = d.StudentTc,
        ParentTc = d.ParentTc,
        School = d.School,
        Grade = d.Grade,
        ServiceType = d.ServiceType,
        ParentName = d.ParentName,
        Phone1 = d.Phone1,
        Phone2 = d.Phone2,
        Address = d.Address,
        Area = d.Area,
        Latitude = d.Latitude,
        Longitude = d.Longitude,
        HealthNote = d.HealthNote,
        KvkkAccepted = d.KvkkAccepted,
        SignedBy = d.SignedBy,
        PaymentStatus = string.IsNullOrEmpty(d.PaymentStatus) ? "Beklemede" : d.PaymentStatus,
        Fee = d.Fee,
        PaymentMethod = d.PaymentMethod,
        Bank = d.Bank,
        DownPayment = d.DownPayment,
        DownPaymentMethod = d.DownPaymentMethod,
        DownPaymentBank = d.DownPaymentBank,
        InstallmentAmount = d.InstallmentAmount,
        InstallmentBank = d.InstallmentBank,
        Installments = d.Installments,
        // "Alınan tutar" = peşin + taksit (plan girildiyse); yoksa gönderilen PaidAmount
        PaidAmount = (d.DownPayment.HasValue || d.InstallmentAmount.HasValue)
            ? (d.DownPayment ?? 0) + (d.InstallmentAmount ?? 0)
            : d.PaidAmount,
        // CreatedAt = UtcNow entity'de varsayılan geliyor
    };

    /// <summary>EditDto -> mevcut Entity'nin temel bilgilerini güncelle (tam değişim).</summary>
    public static void ApplyEdit(this Registration r, RegistrationEditDto d)
    {
        r.StudentName = d.StudentName;
        r.StudentTc = d.StudentTc;
        r.ParentTc = d.ParentTc;
        r.School = d.School;
        r.Grade = d.Grade;
        r.ServiceType = d.ServiceType;
        r.ParentName = d.ParentName;
        r.Phone1 = d.Phone1;
        r.Phone2 = d.Phone2;
        r.Address = d.Address;
        r.Area = d.Area;
        r.Latitude = d.Latitude;
        r.Longitude = d.Longitude;
        r.HealthNote = d.HealthNote;
    }

    /// <summary>UpdateDto -> mevcut Entity üzerine uygula (sadece gönderilen/null olmayan alanlar).</summary>
    public static void ApplyUpdate(this Registration r, RegistrationUpdateDto d)
    {
        if (d.PaymentStatus is not null) r.PaymentStatus = d.PaymentStatus;
        if (d.Fee.HasValue) r.Fee = d.Fee;
        if (d.PaymentMethod is not null) r.PaymentMethod = d.PaymentMethod;
        if (d.Bank is not null) r.Bank = d.Bank;
        if (d.DownPayment.HasValue) r.DownPayment = d.DownPayment;
        if (d.DownPaymentMethod is not null) r.DownPaymentMethod = d.DownPaymentMethod;
        if (d.DownPaymentBank is not null) r.DownPaymentBank = d.DownPaymentBank;
        if (d.InstallmentAmount.HasValue) r.InstallmentAmount = d.InstallmentAmount;
        if (d.InstallmentBank is not null) r.InstallmentBank = d.InstallmentBank;
        if (d.Installments.HasValue) r.Installments = d.Installments;
        // "Alınan tutar" = peşin + taksit (plan alanı geldiyse); yoksa gönderilen PaidAmount
        if (d.DownPayment.HasValue || d.InstallmentAmount.HasValue)
            r.PaidAmount = (r.DownPayment ?? 0) + (r.InstallmentAmount ?? 0);
        else if (d.PaidAmount.HasValue) r.PaidAmount = d.PaidAmount;
        if (d.Notes is not null) r.Notes = d.Notes;
        if (d.BoardStatus is not null) r.BoardStatus = d.BoardStatus;
        if (d.SkipTomorrow.HasValue) r.SkipTomorrow = d.SkipTomorrow.Value;
        if (d.VehicleId.HasValue) r.VehicleId = d.VehicleId.Value;
        r.SecondVehicleId = d.SecondVehicleId;   // her zaman uygula (null = ikinci aracı kaldır)
    }
}
