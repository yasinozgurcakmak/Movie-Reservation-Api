using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(PasswordResetToken token);
    Task DeleteByUserIdAsync(int userId);
    Task SaveChangesAsync();
}
