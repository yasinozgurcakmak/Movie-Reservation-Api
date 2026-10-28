using DtoLayer.Reservations;
using EntityLayer.Entities;

namespace BusinessLayer.Mappings;

public static class ReservationMappings
{
    public static ReservationResponse ToResponse(this Reservation reservation) => new(
        reservation.Id,
        reservation.ShowtimeId,
        reservation.Showtime.Movie.Title,
        reservation.Showtime.Hall.Name,
        reservation.Showtime.StartTime,
        reservation.Seats.OrderBy(s => s.Id).Select(s => s.SeatCode).ToList(),
        reservation.TotalAmount,
        reservation.Status.ToString(),
        reservation.CreatedAt);
}
