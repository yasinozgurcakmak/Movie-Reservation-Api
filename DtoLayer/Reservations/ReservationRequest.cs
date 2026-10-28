namespace DtoLayer.Reservations;

public record ReservationRequest(int ShowtimeId, List<string> SeatCodes);
