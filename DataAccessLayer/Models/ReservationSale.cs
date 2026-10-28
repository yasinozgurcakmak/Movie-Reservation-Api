namespace DataAccessLayer.Models;

public record ReservationSale(DateTimeOffset CreatedAt, int SeatCount, decimal TotalAmount);
