namespace EntityLayer.Entities;

public class ReservationSeat
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public int ShowtimeId { get; set; }
    public string SeatCode { get; set; } = null!;
    public bool IsCancelled { get; set; }
}
