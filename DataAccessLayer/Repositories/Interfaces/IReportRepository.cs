using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IReportRepository
{
    Task<List<ShowtimeSalesSummary>> GetShowtimeSalesAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);
    Task<List<MovieGenrePair>> GetMovieGenresAsync(IEnumerable<int> movieIds, CancellationToken cancellationToken = default);
    Task<List<ReservationSale>> GetConfirmedSalesAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
}
