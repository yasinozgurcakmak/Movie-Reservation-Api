using EntityLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<List<string>> GetReservedSeatCodesAsync(int showtimeId);
    Task<List<Reservation>> GetByUserIdAsync(int userId);
    Task<Reservation?> GetByIdForUserAsync(int id, int userId);
    Task AddAsync(Reservation reservation);
    Task SaveChangesAsync();
}
