using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash) => _context.PasswordResetTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    public async Task AddAsync(PasswordResetToken token) => await _context.PasswordResetTokens.AddAsync(token);
    public Task DeleteByUserIdAsync(int userId) => _context.PasswordResetTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
