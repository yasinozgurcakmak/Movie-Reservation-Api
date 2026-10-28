using System.Globalization;
using System.Text;
using System.Text.Json;
using BusinessLayer.Interfaces;
using BusinessLayer.Options;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Reports;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Concrete;

public class ReportManager : IReportService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    private const string Matinee = "Matinee (06:00-16:59)";
    private const string Evening = "Evening (17:00-20:59)";
    private const string Night = "Night (21:00-05:59)";
    private static readonly string[] TimeSlotNames = [Matinee, Evening, Night];

    private readonly IReportRepository _reportRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ReportManager> _logger;
    private readonly IValidator<AnalyticsQueryParameters> _analyticsValidator;
    private readonly IValidator<SalesQueryParameters> _salesValidator;
    private readonly TimeZoneInfo _cinemaTimeZone;

    public ReportManager(
        IReportRepository reportRepository,
        IDistributedCache cache,
        ILogger<ReportManager> logger,
        IValidator<AnalyticsQueryParameters> analyticsValidator,
        IValidator<SalesQueryParameters> salesValidator,
        IOptions<CinemaSettings> options)
    {
        _reportRepository = reportRepository;
        _cache = cache;
        _logger = logger;
        _analyticsValidator = analyticsValidator;
        _salesValidator = salesValidator;
        _cinemaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);
    }

    public async Task<AnalyticsResponse> GetAnalyticsAsync(AnalyticsQueryParameters query)
    {
        await _analyticsValidator.ValidateAndThrowAsync(query);

        var cacheKey = $"reports:analytics:{query.From:yyyy-MM-dd}:{query.To:yyyy-MM-dd}:{query.Top}";
        return await GetOrCreateCachedAsync(cacheKey, () => BuildAnalyticsAsync(query));
    }

    public async Task<byte[]> GetAnalyticsCsvAsync(AnalyticsQueryParameters query)
    {
        var analytics = await GetAnalyticsAsync(query);

        var csv = new StringBuilder();
        csv.AppendLine("ShowtimeId,StartTime,Movie,Hall,Capacity,SoldSeats,OccupancyRate,Revenue,CancelledSeats,LostRevenue");
        foreach (var s in analytics.Showtimes)
        {
            csv.AppendLine(string.Join(',',
                s.ShowtimeId.ToString(CultureInfo.InvariantCulture),
                ToLocal(s.StartTime).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                EscapeCsv(s.MovieTitle),
                EscapeCsv(s.HallName),
                s.Capacity.ToString(CultureInfo.InvariantCulture),
                s.SoldSeats.ToString(CultureInfo.InvariantCulture),
                s.OccupancyRate.ToString(CultureInfo.InvariantCulture),
                s.Revenue.ToString("0.00", CultureInfo.InvariantCulture),
                s.CancelledSeats.ToString(CultureInfo.InvariantCulture),
                s.LostRevenue.ToString("0.00", CultureInfo.InvariantCulture)));
        }

        return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
    }

    public async Task<SalesReportResponse> GetSalesAsync(SalesQueryParameters query)
    {
        await _salesValidator.ValidateAndThrowAsync(query);

        var interval = query.Interval.ToLowerInvariant();
        var cacheKey = $"reports:sales:{query.From:yyyy-MM-dd}:{query.To:yyyy-MM-dd}:{interval}";
        return await GetOrCreateCachedAsync(cacheKey, () => BuildSalesAsync(query.From!.Value, query.To!.Value, interval));
    }

    private async Task<AnalyticsResponse> BuildAnalyticsAsync(AnalyticsQueryParameters query)
    {
        DateTimeOffset? from = query.From.HasValue ? ToUtcStartOfDay(query.From.Value) : null;
        DateTimeOffset? to = query.To.HasValue ? ToUtcStartOfDay(query.To.Value.AddDays(1)) : null;

        var showtimes = await _reportRepository.GetShowtimeSalesAsync(from, to);
        var movieGenres = await _reportRepository.GetMovieGenresAsync(showtimes.Select(s => s.MovieId).Distinct().ToList());

        var movies = showtimes
            .GroupBy(s => new { s.MovieId, s.MovieTitle })
            .Select(g =>
            {
                var soldSeats = g.Sum(s => s.SoldSeats);
                var cancelledSeats = g.Sum(s => s.CancelledSeats);
                return new MovieAnalyticsResponse(
                    g.Key.MovieId,
                    g.Key.MovieTitle,
                    g.Count(),
                    soldSeats,
                    g.Sum(s => s.Capacity),
                    Percentage(soldSeats, g.Sum(s => s.Capacity)),
                    g.Sum(s => s.Revenue),
                    cancelledSeats,
                    Percentage(cancelledSeats, soldSeats + cancelledSeats),
                    g.Sum(s => s.LostRevenue));
            })
            .OrderByDescending(m => m.Revenue)
            .ThenBy(m => m.Title)
            .ToList();

        var showtimeResponses = showtimes
            .Select(s => new ShowtimeAnalyticsResponse(
                s.ShowtimeId,
                s.MovieId,
                s.MovieTitle,
                s.HallId,
                s.HallName,
                s.StartTime,
                s.SoldSeats,
                s.Capacity,
                Percentage(s.SoldSeats, s.Capacity),
                s.Revenue,
                s.CancelledSeats,
                s.LostRevenue))
            .ToList();

        var halls = showtimes
            .GroupBy(s => new { s.HallId, s.HallName })
            .Select(g => ToSegment(g.Key.HallId, g.Key.HallName, g))
            .OrderBy(h => h.Name)
            .ToList();

        var genres = movieGenres
            .GroupBy(p => new { p.GenreId, p.GenreName })
            .Select(g =>
            {
                var movieIds = g.Select(p => p.MovieId).ToHashSet();
                return ToSegment(g.Key.GenreId, g.Key.GenreName, showtimes.Where(s => movieIds.Contains(s.MovieId)));
            })
            .OrderByDescending(g => g.Revenue)
            .ThenBy(g => g.Name)
            .ToList();

        var timeSlots = TimeSlotNames
            .Select(name => ToSegment(null, name, showtimes.Where(s => TimeSlotOf(s.StartTime) == name)))
            .ToList();

        var totalSold = showtimes.Sum(s => s.SoldSeats);
        var totalCapacity = showtimes.Sum(s => s.Capacity);
        var totalCancelled = showtimes.Sum(s => s.CancelledSeats);

        return new AnalyticsResponse(
            query.From,
            query.To,
            totalSold,
            totalCapacity,
            Percentage(totalSold, totalCapacity),
            showtimes.Sum(s => s.Revenue),
            totalCancelled,
            Percentage(totalCancelled, totalSold + totalCancelled),
            showtimes.Sum(s => s.LostRevenue),
            movies,
            showtimeResponses,
            halls,
            genres,
            timeSlots,
            movies.Take(query.Top).ToList(),
            showtimeResponses.OrderBy(s => s.OccupancyRate).ThenBy(s => s.StartTime).Take(query.Top).ToList());
    }

    private async Task<SalesReportResponse> BuildSalesAsync(DateOnly from, DateOnly to, string interval)
    {
        var sales = await _reportRepository.GetConfirmedSalesAsync(ToUtcStartOfDay(from), ToUtcStartOfDay(to.AddDays(1)));

        DateOnly PeriodStart(DateOnly date) =>
            interval == "week" ? date.AddDays(-(((int)date.DayOfWeek + 6) % 7)) : date;

        var salesByPeriod = sales
            .GroupBy(s => PeriodStart(DateOnly.FromDateTime(ToLocal(s.CreatedAt).DateTime)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var points = new List<SalesPointResponse>();
        var step = interval == "week" ? 7 : 1;
        for (var period = PeriodStart(from); period <= to; period = period.AddDays(step))
        {
            var items = salesByPeriod.GetValueOrDefault(period) ?? [];
            points.Add(new SalesPointResponse(period, items.Count, items.Sum(i => i.SeatCount), items.Sum(i => i.TotalAmount)));
        }

        return new SalesReportResponse(
            from,
            to,
            interval,
            sales.Count,
            sales.Sum(s => s.SeatCount),
            sales.Sum(s => s.TotalAmount),
            points);
    }

    private async Task<T> GetOrCreateCachedAsync<T>(string key, Func<Task<T>> factory)
    {
        try
        {
            var cached = await _cache.GetStringAsync(key);
            if (cached is not null) return JsonSerializer.Deserialize<T>(cached)!;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Report cache read failed for {CacheKey}", key);
        }

        var value = await factory();

        try
        {
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Report cache write failed for {CacheKey}", key);
        }

        return value;
    }

    private static SegmentAnalyticsResponse ToSegment(int? id, string name, IEnumerable<ShowtimeSalesSummary> showtimes)
    {
        var items = showtimes.ToList();
        var soldSeats = items.Sum(s => s.SoldSeats);
        var capacity = items.Sum(s => s.Capacity);
        return new SegmentAnalyticsResponse(id, name, items.Count, soldSeats, capacity, Percentage(soldSeats, capacity), items.Sum(s => s.Revenue));
    }

    private string TimeSlotOf(DateTimeOffset startTime) => ToLocal(startTime).Hour switch
    {
        >= 6 and < 17 => Matinee,
        >= 17 and < 21 => Evening,
        _ => Night
    };

    private DateTimeOffset ToUtcStartOfDay(DateOnly date) =>
        new(TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), _cinemaTimeZone));

    private DateTimeOffset ToLocal(DateTimeOffset value) => TimeZoneInfo.ConvertTime(value, _cinemaTimeZone);

    private static decimal Percentage(int part, int total) =>
        total == 0 ? 0 : Math.Round(part * 100m / total, 2);

    private static string EscapeCsv(string value)
    {
        if (value.Length > 0 && "=+-@".Contains(value[0])) value = "'" + value;
        return value.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
