using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _context;

    public MovieRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Movie> Items, int TotalCount)> GetPagedAsync(int? genreId, int page, int pageSize)
    {
        var query = _context.Movies.AsNoTracking();
        if (genreId.HasValue) query = query.Where(m => m.Genres.Any(g => g.Id == genreId.Value));
        var totalCount = await query.CountAsync();
        var items = await query.Include(m => m.Genres).OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public Task<Movie?> GetByIdAsync(int id) => _context.Movies.Include(m => m.Genres).FirstOrDefaultAsync(m => m.Id == id);
    public Task<Movie?> GetByIdIncludingDeletedAsync(int id) => _context.Movies.IgnoreQueryFilters().Include(m => m.Genres).FirstOrDefaultAsync(m => m.Id == id);
    public async Task AddAsync(Movie movie) => await _context.Movies.AddAsync(movie);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
