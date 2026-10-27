using DtoLayer.Halls;
using EntityLayer.Entities;

namespace BusinessLayer.Mappings;

public static class HallMappings
{
    public static HallResponse ToResponse(this Hall hall) => new(hall.Id, hall.Name, hall.RowCount, hall.SeatsPerRow);
}
