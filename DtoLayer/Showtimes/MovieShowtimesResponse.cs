namespace DtoLayer.Showtimes;

public record MovieShowtimesResponse(
    int MovieId,
    string Title,
    string PosterUrl,
    int DurationMinutes,
    List<ShowtimeSlotResponse> Showtimes);

public record ShowtimeSlotResponse(
    int Id,
    int HallId,
    string HallName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal TicketPrice);
