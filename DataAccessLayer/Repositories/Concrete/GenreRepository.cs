using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class GenreRepository : IGenreRepository
{
    private readonly AppDbContext _context;

    public GenreRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Genres.AsNoTracking().OrderBy(g => g.Name).ToListAsync(cancellationToken);
    public Task<List<Genre>> GetByIdsAsync(IEnumerable<int> ids) => _context.Genres.Where(g => ids.Contains(g.Id)).ToListAsync();
    public Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Genres.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        _context.Genres.AnyAsync(g => g.Name.ToLower() == name.ToLower() && g.Id != excludeId);
    public Task<int> CountMoviesUsingAsync(int id) =>
        _context.Movies.IgnoreQueryFilters().CountAsync(m => m.Genres.Any(g => g.Id == id));
    public async Task AddAsync(Genre genre) => await _context.Genres.AddAsync(genre);
    public void Remove(Genre genre) => _context.Genres.Remove(genre);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
