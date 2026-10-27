using DtoLayer.Showtimes;
using EntityLayer.Entities;

namespace BusinessLayer.Mappings;

public static class ShowtimeMappings
{
    public static ShowtimeResponse ToResponse(this Showtime showtime) => new(
        showtime.Id,
        showtime.MovieId,
        showtime.Movie.Title,
        showtime.HallId,
        showtime.Hall.Name,
        showtime.StartTime,
        showtime.EndTime,
        showtime.TicketPrice);

    public static ShowtimeSlotResponse ToSlotResponse(this Showtime showtime) => new(
        showtime.Id,
        showtime.HallId,
        showtime.Hall.Name,
        showtime.StartTime,
        showtime.EndTime,
        showtime.TicketPrice);
}
