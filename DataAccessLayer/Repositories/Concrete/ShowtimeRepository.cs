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

    public Task<Showtime?> GetByIdAsync(int id) =>
        _context.Showtimes.AsNoTracking().Include(s => s.Movie).Include(s => s.Hall).FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Showtime>> GetByStartTimeRangeAsync(DateTimeOffset from, DateTimeOffset to) =>
        _context.Showtimes.AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartTime >= from && s.StartTime < to)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

   
    public Task<bool> HasOverlapAsync(int hallId, DateTimeOffset start, DateTimeOffset end) =>
        _context.Showtimes.AnyAsync(s => s.HallId == hallId && s.StartTime < end && s.EndTime > start);

    public async Task AddAsync(Showtime showtime) => await _context.Showtimes.AddAsync(showtime);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
