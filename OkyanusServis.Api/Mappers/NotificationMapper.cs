using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Mappers;

public static class NotificationMapper
{
    public static NotificationDto ToDto(this Notification n) => new()
    {
        Id = n.Id,
        RegistrationId = n.RegistrationId,
        Icon = n.Icon,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
    };

    public static Notification ToEntity(this NotificationCreateDto d) => new()
    {
        RegistrationId = d.RegistrationId,
        Icon = d.Icon,
        Title = d.Title,
        Message = d.Message,
    };
}
