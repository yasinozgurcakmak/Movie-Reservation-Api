using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IMovieRepository
{
    Task<(List<Movie> Items, int TotalCount)> GetPagedAsync(int? genreId, int page, int pageSize);
    Task<Movie?> GetByIdAsync(int id);
    Task<Movie?> GetByIdIncludingDeletedAsync(int id);
    Task AddAsync(Movie movie);
    Task SaveChangesAsync();
}
