using EntityLayer.Enums;

namespace EntityLayer.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ShowtimeId { get; set; }
    public Showtime Showtime { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<ReservationSeat> Seats { get; set; } = new();
}
