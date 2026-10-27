namespace DtoLayer.Showtimes;

public record SeatMapResponse(
    int ShowtimeId,
    int HallId,
    string HallName,
    List<SeatRowResponse> Rows);

public record SeatRowResponse(string Row, List<SeatResponse> Seats);

public record SeatResponse(string Code, bool IsAvailable);
