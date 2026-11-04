using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using EntityLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<string>> GetReservedSeatCodesAsync(int showtimeId, CancellationToken cancellationToken = default) =>
        _context.ReservationSeats.Where(s => s.ShowtimeId == showtimeId && !s.IsCancelled).Select(s => s.SeatCode).ToListAsync(cancellationToken);
    public Task<int> CountActiveSeatsAsync(int showtimeId, int userId) =>
        _context.ReservationSeats.CountAsync(s => s.ShowtimeId == showtimeId && !s.IsCancelled && s.Reservation.UserId == userId);
    public Task<bool> HasActiveReservationsAsync(int showtimeId) =>
        _context.Reservations.AnyAsync(r => r.ShowtimeId == showtimeId && r.Status == ReservationStatus.Confirmed);
    public Task<List<Reservation>> GetActiveByShowtimeIdAsync(int showtimeId) =>
        _context.Reservations.Include(r => r.User).Include(r => r.Seats)
            .Where(r => r.ShowtimeId == showtimeId && r.Status == ReservationStatus.Confirmed)
            .ToListAsync();
    public Task<List<Reservation>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        WithDetails().AsNoTracking().Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
    public Task<Reservation?> GetByIdForUserAsync(int id, int userId) =>
        WithDetails().FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

    public async Task<(List<Reservation> Items, int TotalCount)> GetPagedAsync(
        ReservationStatus? status, int? showtimeId, int? userId, string? email, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = WithDetails().Include(r => r.User).AsNoTracking();
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (showtimeId.HasValue) query = query.Where(r => r.ShowtimeId == showtimeId.Value);
        if (userId.HasValue) query = query.Where(r => r.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(email)) query = query.Where(r => r.User.Email.ToLower().Contains(email.Trim().ToLower()));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddAsync(Reservation reservation) => await _context.Reservations.AddAsync(reservation);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    // Reservation history must survive soft-deleted movies, so the showtime query filter is ignored.
    private IQueryable<Reservation> WithDetails() =>
        _context.Reservations.IgnoreQueryFilters()
            .Include(r => r.Showtime).ThenInclude(s => s.Movie)
            .Include(r => r.Showtime).ThenInclude(s => s.Hall)
            .Include(r => r.Seats);
}
