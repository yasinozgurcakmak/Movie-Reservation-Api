using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IHallRepository
{
    Task<List<Hall>> GetAllAsync();
    Task<Hall?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<int> CountShowtimesAsync(int id);
    Task AddAsync(Hall hall);
    void Remove(Hall hall);
    Task SaveChangesAsync();
}
