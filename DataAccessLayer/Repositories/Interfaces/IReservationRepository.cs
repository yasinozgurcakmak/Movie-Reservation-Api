using EntityLayer.Entities;
using EntityLayer.Enums;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<List<string>> GetReservedSeatCodesAsync(int showtimeId, CancellationToken cancellationToken = default);
    Task<int> CountActiveSeatsAsync(int showtimeId, int userId);
    Task<bool> HasActiveReservationsAsync(int showtimeId);
    Task<List<Reservation>> GetActiveByShowtimeIdAsync(int showtimeId);
    Task<List<Reservation>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Reservation?> GetByIdForUserAsync(int id, int userId);
    Task<(List<Reservation> Items, int TotalCount)> GetPagedAsync(
        ReservationStatus? status, int? showtimeId, int? userId, string? email, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation);
    Task SaveChangesAsync();
}
