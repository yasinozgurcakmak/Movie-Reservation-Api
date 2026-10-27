namespace DtoLayer.Showtimes;

public record ShowtimeRequest(int MovieId, int HallId, DateTimeOffset StartTime, decimal TicketPrice);
