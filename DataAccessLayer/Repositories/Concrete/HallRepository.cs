using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class HallRepository : IHallRepository
{
    private readonly AppDbContext _context;

    public HallRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Hall>> GetAllAsync() => _context.Halls.AsNoTracking().OrderBy(h => h.Name).ToListAsync();
    public Task<Hall?> GetByIdAsync(int id) => _context.Halls.FirstOrDefaultAsync(h => h.Id == id);
    public Task<bool> NameExistsAsync(string name, int? excludeId = null) =>
        _context.Halls.AnyAsync(h => h.Name.ToLower() == name.ToLower() && h.Id != excludeId);
    public Task<int> CountShowtimesAsync(int id) =>
        _context.Showtimes.IgnoreQueryFilters().CountAsync(s => s.HallId == id);
    public async Task AddAsync(Hall hall) => await _context.Halls.AddAsync(hall);
    public void Remove(Hall hall) => _context.Halls.Remove(hall);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
