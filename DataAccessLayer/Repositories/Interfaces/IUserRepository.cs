using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<string?> GetSecurityStampAsync(int id);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
