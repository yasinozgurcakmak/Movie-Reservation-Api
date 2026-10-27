using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IShowtimeRepository
{
    Task<Showtime?> GetByIdAsync(int id);
    Task<List<Showtime>> GetByStartTimeRangeAsync(DateTimeOffset from, DateTimeOffset to);
    Task<bool> HasOverlapAsync(int hallId, DateTimeOffset start, DateTimeOffset end);
    Task AddAsync(Showtime showtime);
    Task SaveChangesAsync();
}
