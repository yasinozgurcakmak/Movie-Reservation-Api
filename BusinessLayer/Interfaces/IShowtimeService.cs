using DtoLayer.Showtimes;

namespace BusinessLayer.Interfaces;

public interface IShowtimeService
{
    Task<List<MovieShowtimesResponse>> GetByDateAsync(ShowtimeQueryParameters query);
    Task<ShowtimeResponse> GetByIdAsync(int id);
    Task<SeatMapResponse> GetSeatMapAsync(int id);
    Task<ShowtimeResponse> CreateAsync(ShowtimeRequest request);
}
