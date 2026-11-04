using System.Net;
using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using BusinessLayer.Options;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Showtimes;
using EntityLayer.Entities;
using EntityLayer.Enums;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Concrete;

public class ShowtimeManager : IShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IValidator<ShowtimeRequest> _showtimeValidator;
    private readonly IValidator<ShowtimeQueryParameters> _queryValidator;
    private readonly CatalogCache _catalogCache;
    private readonly IEmailQueue _emailQueue;
    private readonly TimeZoneInfo _cinemaTimeZone;

    public ShowtimeManager(
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository,
        IHallRepository hallRepository,
        IReservationRepository reservationRepository,
        IValidator<ShowtimeRequest> showtimeValidator,
        IValidator<ShowtimeQueryParameters> queryValidator,
        CatalogCache catalogCache,
        IEmailQueue emailQueue,
        IOptions<CinemaSettings> options)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _hallRepository = hallRepository;
        _reservationRepository = reservationRepository;
        _showtimeValidator = showtimeValidator;
        _queryValidator = queryValidator;
        _catalogCache = catalogCache;
        _emailQueue = emailQueue;
        _cinemaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);
    }

    public async Task<List<MovieShowtimesResponse>> GetByDateAsync(ShowtimeQueryParameters query, CancellationToken cancellationToken = default)
    {
        await _queryValidator.ValidateAndThrowAsync(query);

        var date = query.Date!.Value;
        return await _catalogCache.GetOrCreateAsync($"showtimes:date:{date:yyyy-MM-dd}", () => BuildDailyListingAsync(date, cancellationToken));
    }

    private async Task<List<MovieShowtimesResponse>> BuildDailyListingAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var localDayStart = date.ToDateTime(TimeOnly.MinValue);
        var from = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDayStart, _cinemaTimeZone));
        var to = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDayStart.AddDays(1), _cinemaTimeZone));

        var showtimes = await _showtimeRepository.GetByStartTimeRangeAsync(from, to, cancellationToken);

        return showtimes
            .GroupBy(s => s.MovieId)
            .Select(g =>
            {
                var movie = g.First().Movie;
                return new MovieShowtimesResponse(
                    movie.Id,
                    movie.Title,
                    movie.PosterUrl,
                    movie.DurationMinutes,
                    g.Select(s => s.ToSlotResponse()).ToList());
            })
            .OrderBy(m => m.Title)
            .ToList();
    }

    public async Task<ShowtimeResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Showtime with id {id} was not found.");
        return showtime.ToResponse();
    }

    public async Task<SeatMapResponse> GetSeatMapAsync(int id, CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Showtime with id {id} was not found.");
        var hall = showtime.Hall;
        var reservedSeats = (await _reservationRepository.GetReservedSeatCodesAsync(id, cancellationToken)).ToHashSet();

        var rows = Enumerable.Range(0, hall.RowCount)
            .Select(rowIndex =>
            {
                var row = SeatLayout.ToRowLabel(rowIndex);
                var seats = Enumerable.Range(1, hall.SeatsPerRow)
                    .Select(number => SeatLayout.ToSeatCode(row, number))
                    .Select(code => new SeatResponse(code, IsAvailable: !reservedSeats.Contains(code)))
                    .ToList();
                return new SeatRowResponse(row, seats);
            })
            .ToList();

        return new SeatMapResponse(showtime.Id, hall.Id, hall.Name, rows);
    }

    public async Task<ShowtimeResponse> CreateAsync(ShowtimeRequest request)
    {
        await _showtimeValidator.ValidateAndThrowAsync(request);

        var showtime = new Showtime { CreatedAt = DateTimeOffset.UtcNow };
        await ApplyRequestAsync(showtime, request);

        await _showtimeRepository.AddAsync(showtime);
        await _showtimeRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return showtime.ToResponse();
    }

    public async Task<ShowtimeResponse> UpdateAsync(int id, ShowtimeRequest request)
    {
        await _showtimeValidator.ValidateAndThrowAsync(request);

        var showtime = await GetUpcomingShowtimeAsync(id, "changed");

        // Sold tickets name the hall, time and price, so they cannot change under the customer.
        if (await _reservationRepository.HasActiveReservationsAsync(id))
            throw new ConflictException($"Showtime with id {id} has reservations and cannot be changed. Cancel it and create a new one instead.");

        await ApplyRequestAsync(showtime, request);

        await _showtimeRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        return showtime.ToResponse();
    }

    public async Task CancelAsync(int id)
    {
        var showtime = await GetUpcomingShowtimeAsync(id, "cancelled");
        var reservations = await _reservationRepository.GetActiveByShowtimeIdAsync(id);

        foreach (var reservation in reservations)
        {
            reservation.Status = ReservationStatus.Cancelled;
            reservation.Seats.ForEach(s => s.IsCancelled = true);
        }

        showtime.IsCancelled = true;
        await _showtimeRepository.SaveChangesAsync();
        await _catalogCache.InvalidateAsync();

        var localStart = TimeZoneInfo.ConvertTime(showtime.StartTime, _cinemaTimeZone);
        foreach (var reservation in reservations)
        {
            var body = $"""
                <p>Merhaba, {WebUtility.HtmlEncode(reservation.User.FirstName)},</p>
                <p>{WebUtility.HtmlEncode(showtime.Movie.Title)} filminin {localStart:dd.MM.yyyy HH:mm} seansı iptal edildi.</p>
                <p>{string.Join(", ", reservation.Seats.Select(s => s.SeatCode))} numaralı koltuklar için yaptığınız rezervasyon iptal edilmiştir.</p>
                """;
            _emailQueue.Enqueue(new EmailMessage(reservation.User.Email, "Your showtime has been cancelled", body));
        }
    }

    private async Task<Showtime> GetUpcomingShowtimeAsync(int id, string action)
    {
        var showtime = await _showtimeRepository.GetByIdForUpdateAsync(id)
            ?? throw new NotFoundException($"Showtime with id {id} was not found.");

        if (showtime.StartTime <= DateTimeOffset.UtcNow)
            throw new BadRequestException($"Showtimes that have already started cannot be {action}.");

        return showtime;
    }

    private async Task ApplyRequestAsync(Showtime showtime, ShowtimeRequest request)
    {
        var movie = await _movieRepository.GetByIdAsync(request.MovieId)
            ?? throw new BadRequestException($"Movie with id {request.MovieId} was not found.");
        var hall = await _hallRepository.GetByIdAsync(request.HallId)
            ?? throw new BadRequestException($"Hall with id {request.HallId} was not found.");

        var startTime = request.StartTime.ToUniversalTime();
        var endTime = startTime.AddMinutes(movie.DurationMinutes);

        int? excludeId = showtime.Id == 0 ? null : showtime.Id;
        if (await _showtimeRepository.HasOverlapAsync(hall.Id, startTime, endTime, excludeId))
            throw new ConflictException($"Hall '{hall.Name}' already has a showtime overlapping {startTime:u} - {endTime:u}.");

        showtime.Movie = movie;
        showtime.Hall = hall;
        showtime.StartTime = startTime;
        showtime.EndTime = endTime;
        showtime.TicketPrice = request.TicketPrice;
    }
}
