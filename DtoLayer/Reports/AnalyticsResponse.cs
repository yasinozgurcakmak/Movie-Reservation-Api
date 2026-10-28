namespace DtoLayer.Reports;

public record AnalyticsResponse(
    DateOnly? From,
    DateOnly? To,
    int SoldSeats,
    int Capacity,
    decimal OccupancyRate,
    decimal TotalRevenue,
    int CancelledSeats,
    decimal CancellationRate,
    decimal LostRevenue,
    List<MovieAnalyticsResponse> Movies,
    List<ShowtimeAnalyticsResponse> Showtimes,
    List<SegmentAnalyticsResponse> Halls,
    List<SegmentAnalyticsResponse> Genres,
    List<SegmentAnalyticsResponse> TimeSlots,
    List<MovieAnalyticsResponse> TopMoviesByRevenue,
    List<ShowtimeAnalyticsResponse> LeastOccupiedShowtimes);

public record MovieAnalyticsResponse(
    int MovieId,
    string Title,
    int ShowtimeCount,
    int SoldSeats,
    int Capacity,
    decimal OccupancyRate,
    decimal Revenue,
    int CancelledSeats,
    decimal CancellationRate,
    decimal LostRevenue);

public record ShowtimeAnalyticsResponse(
    int ShowtimeId,
    int MovieId,
    string MovieTitle,
    int HallId,
    string HallName,
    DateTimeOffset StartTime,
    int SoldSeats,
    int Capacity,
    decimal OccupancyRate,
    decimal Revenue,
    int CancelledSeats,
    decimal LostRevenue);

public record SegmentAnalyticsResponse(
    int? Id,
    string Name,
    int ShowtimeCount,
    int SoldSeats,
    int Capacity,
    decimal OccupancyRate,
    decimal Revenue);
