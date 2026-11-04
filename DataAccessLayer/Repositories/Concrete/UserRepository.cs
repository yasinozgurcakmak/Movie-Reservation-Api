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

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    public Task<User?> GetByEmailAsync(string email) => _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    public Task<bool> EmailExistsAsync(string email) => _context.Users.AnyAsync(u => u.Email == email);
    public Task<string?> GetSecurityStampAsync(int id) =>
        _context.Users.Where(u => u.Id == id).Select(u => u.SecurityStamp).FirstOrDefaultAsync();
    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(cancellationToken);
    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
