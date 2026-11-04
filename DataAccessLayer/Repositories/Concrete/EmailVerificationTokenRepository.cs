using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Concrete;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AppDbContext _context;

    public EmailVerificationTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash) => _context.EmailVerificationTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    public async Task AddAsync(EmailVerificationToken token) => await _context.EmailVerificationTokens.AddAsync(token);
    public Task DeleteByUserIdAsync(int userId) => _context.EmailVerificationTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
