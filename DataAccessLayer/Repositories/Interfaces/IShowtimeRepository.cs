using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IShowtimeRepository
{
    Task<Showtime?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Showtime?> GetByIdForUpdateAsync(int id);
    Task<List<Showtime>> GetByStartTimeRangeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(int hallId, DateTimeOffset start, DateTimeOffset end, int? excludeId = null);
    Task AddAsync(Showtime showtime);
    Task SaveChangesAsync();
}
