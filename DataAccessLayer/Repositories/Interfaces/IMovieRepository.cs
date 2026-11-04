using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IMovieRepository
{
    Task<(List<Movie> Items, int TotalCount)> GetPagedAsync(int? genreId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdIncludingDeletedAsync(int id);
    Task<bool> HasUpcomingReservationsAsync(int id, DateTimeOffset now);
    Task AddAsync(Movie movie);
    Task SaveChangesAsync();
}
