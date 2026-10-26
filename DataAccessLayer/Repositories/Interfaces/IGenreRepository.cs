using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync();
    Task<List<Genre>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Genre?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<int> CountMoviesUsingAsync(int id);
    Task AddAsync(Genre genre);
    void Remove(Genre genre);
    Task SaveChangesAsync();
}
