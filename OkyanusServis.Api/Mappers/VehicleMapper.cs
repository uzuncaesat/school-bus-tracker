using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Mappers;

public static class VehicleMapper
{
    public static VehicleDto ToDto(this Vehicle v) => new()
    {
        Id = v.Id,
        Plate = v.Plate,
        Driver = v.Driver,
        Phone = v.Phone,
        Capacity = v.Capacity,
        Color = v.Color,
        ArventoNode = v.ArventoNode,
        AssignedCount = v.Registrations?.Count ?? 0,   // atanmış öğrenci sayısı
    };

    public static Vehicle ToEntity(this VehicleCreateDto d) => new()
    {
        Plate = d.Plate,
        Driver = d.Driver,
        Phone = d.Phone,
        Capacity = d.Capacity,
        Color = d.Color,
        ArventoNode = string.IsNullOrWhiteSpace(d.ArventoNode) ? null : d.ArventoNode.Trim(),
    };

    public static void ApplyUpdate(this Vehicle v, VehicleUpdateDto d)
    {
        v.Plate = d.Plate;
        v.Driver = d.Driver;
        v.Phone = d.Phone;
        v.Capacity = d.Capacity;
        v.Color = d.Color;
        v.ArventoNode = string.IsNullOrWhiteSpace(d.ArventoNode) ? null : d.ArventoNode.Trim();
    }
}
