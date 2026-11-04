using DtoLayer.Showtimes;

namespace BusinessLayer.Interfaces;

public interface IShowtimeService
{
    Task<List<MovieShowtimesResponse>> GetByDateAsync(ShowtimeQueryParameters query, CancellationToken cancellationToken = default);
    Task<ShowtimeResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SeatMapResponse> GetSeatMapAsync(int id, CancellationToken cancellationToken = default);
    Task<ShowtimeResponse> CreateAsync(ShowtimeRequest request);
    Task<ShowtimeResponse> UpdateAsync(int id, ShowtimeRequest request);
    Task CancelAsync(int id);
}
