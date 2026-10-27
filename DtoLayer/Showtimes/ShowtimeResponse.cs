namespace DtoLayer.Showtimes;

public record ShowtimeResponse(
    int Id,
    int MovieId,
    string MovieTitle,
    int HallId,
    string HallName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal TicketPrice);
