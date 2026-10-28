using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IReportRepository
{
    Task<List<ShowtimeSalesSummary>> GetShowtimeSalesAsync(DateTimeOffset? from, DateTimeOffset? to);
    Task<List<MovieGenrePair>> GetMovieGenresAsync(IEnumerable<int> movieIds);
    Task<List<ReservationSale>> GetConfirmedSalesAsync(DateTimeOffset from, DateTimeOffset to);
}
