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

    public static AdminReservationResponse ToAdminResponse(this Reservation reservation) => new(
        reservation.Id,
        reservation.UserId,
        $"{reservation.User.FirstName} {reservation.User.LastName}",
        reservation.User.Email,
        reservation.Showtime.MovieId,
        reservation.Showtime.Movie.Title,
        reservation.ShowtimeId,
        reservation.Showtime.Hall.Name,
        reservation.Showtime.StartTime,
        reservation.Seats.OrderBy(s => s.Id).Select(s => s.SeatCode).ToList(),
        reservation.TotalAmount,
        reservation.Status.ToString(),
        reservation.CreatedAt);
}
