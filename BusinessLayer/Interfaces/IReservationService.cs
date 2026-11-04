using DtoLayer.Common;
using DtoLayer.Reservations;

namespace BusinessLayer.Interfaces;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(int userId, ReservationRequest request);
    Task<List<ReservationResponse>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<ReservationResponse> UpdateStatusAsync(int userId, int id, ReservationStatusRequest request);
    Task<PagedResponse<AdminReservationResponse>> GetPagedAsync(ReservationQueryParameters query, CancellationToken cancellationToken = default);
}
