using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(EmailVerificationToken token);
    Task DeleteByUserIdAsync(int userId);
    Task SaveChangesAsync();
}
