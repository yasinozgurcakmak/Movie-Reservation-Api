using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(int id) => _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    public Task<User?> GetByEmailAsync(string email) => _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    public Task<bool> EmailExistsAsync(string email) => _context.Users.AnyAsync(u => u.Email == email);
    public Task<List<User>> GetAllAsync() => _context.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
