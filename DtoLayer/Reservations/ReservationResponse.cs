namespace DtoLayer.Reservations;

public record ReservationResponse(
    int Id,
    int ShowtimeId,
    string MovieTitle,
    string HallName,
    DateTimeOffset StartTime,
    List<string> SeatCodes,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAt);
