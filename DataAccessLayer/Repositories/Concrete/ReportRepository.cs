using DataAccessLayer.Context;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    // Aggregation runs in PostgreSQL; only one summary row per showtime is returned.
    // Soft-deleted movies are included because their sales still happened.
    public Task<List<ShowtimeSalesSummary>> GetShowtimeSalesAsync(DateTimeOffset? from, DateTimeOffset? to)
    {
        var showtimes = _context.Showtimes.IgnoreQueryFilters();
        if (from.HasValue) showtimes = showtimes.Where(s => s.StartTime >= from.Value);
        if (to.HasValue) showtimes = showtimes.Where(s => s.StartTime < to.Value);

        return showtimes
            .OrderBy(s => s.StartTime)
            .Select(s => new ShowtimeSalesSummary(
                s.Id,
                s.MovieId,
                s.Movie.Title,
                s.HallId,
                s.Hall.Name,
                s.StartTime,
                s.Hall.RowCount * s.Hall.SeatsPerRow,
                _context.ReservationSeats.Count(rs => rs.ShowtimeId == s.Id && !rs.IsCancelled),
                _context.Reservations
                    .Where(r => r.ShowtimeId == s.Id && r.Status == ReservationStatus.Confirmed)
                    .Sum(r => (decimal?)r.TotalAmount) ?? 0,
                _context.ReservationSeats.Count(rs => rs.ShowtimeId == s.Id && rs.IsCancelled),
                _context.Reservations
                    .Where(r => r.ShowtimeId == s.Id && r.Status == ReservationStatus.Cancelled)
                    .Sum(r => (decimal?)r.TotalAmount) ?? 0))
            .ToListAsync();
    }

    public Task<List<MovieGenrePair>> GetMovieGenresAsync(IEnumerable<int> movieIds) =>
        _context.Movies.IgnoreQueryFilters()
            .Where(m => movieIds.Contains(m.Id))
            .SelectMany(m => m.Genres.Select(g => new MovieGenrePair(m.Id, g.Id, g.Name)))
            .ToListAsync();

    public Task<List<ReservationSale>> GetConfirmedSalesAsync(DateTimeOffset from, DateTimeOffset to) =>
        _context.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed && r.CreatedAt >= from && r.CreatedAt < to)
            .Select(r => new ReservationSale(r.CreatedAt, r.Seats.Count, r.TotalAmount))
            .ToListAsync();
}
