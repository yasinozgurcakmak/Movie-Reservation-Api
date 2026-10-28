using EntityLayer.Entities;
using EntityLayer.Enums;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<List<string>> GetReservedSeatCodesAsync(int showtimeId);
    Task<List<Reservation>> GetByUserIdAsync(int userId);
    Task<Reservation?> GetByIdForUserAsync(int id, int userId);
    Task<(List<Reservation> Items, int TotalCount)> GetPagedAsync(
        ReservationStatus? status, int? showtimeId, int? userId, string? email, int page, int pageSize);
    Task AddAsync(Reservation reservation);
    Task SaveChangesAsync();
}
