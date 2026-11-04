using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class ShowtimeRepository : IShowtimeRepository
{
    private readonly AppDbContext _context;

    public ShowtimeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Showtime?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Showtimes.AsNoTracking().Include(s => s.Movie).Include(s => s.Hall).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Showtime?> GetByIdForUpdateAsync(int id) =>
        _context.Showtimes.Include(s => s.Movie).Include(s => s.Hall).FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Showtime>> GetByStartTimeRangeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default) =>
        _context.Showtimes.AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime >= from && s.StartTime < to)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

       public Task<bool> HasOverlapAsync(int hallId, DateTimeOffset start, DateTimeOffset end, int? excludeId = null) =>
        _context.Showtimes.AnyAsync(s => s.HallId == hallId && s.StartTime < end && s.EndTime > start && s.Id != excludeId);

    public async Task AddAsync(Showtime showtime) => await _context.Showtimes.AddAsync(showtime);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
