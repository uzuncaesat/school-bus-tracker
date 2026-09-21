using OkyanusServis.Api.Dtos;
using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Mappers;

public static class AnnouncementMapper
{
    public static AnnouncementDto ToDto(this Announcement a) => new()
    {
        Id = a.Id,
        Text = a.Text,
        CreatedAt = a.CreatedAt,
    };

    public static Announcement ToEntity(this AnnouncementCreateDto d) => new()
    {
        Text = d.Text,
    };
}
