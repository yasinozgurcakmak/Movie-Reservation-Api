namespace DtoLayer.Reservations;

public record AdminReservationResponse(
    int Id,
    int UserId,
    string UserFullName,
    string UserEmail,
    int MovieId,
    string MovieTitle,
    int ShowtimeId,
    string HallName,
    DateTimeOffset StartTime,
    List<string> SeatCodes,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAt);
