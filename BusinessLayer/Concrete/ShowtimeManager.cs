using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using BusinessLayer.Options;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Showtimes;
using EntityLayer.Entities;
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
    private readonly TimeZoneInfo _cinemaTimeZone;

    public ShowtimeManager(
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository,
        IHallRepository hallRepository,
        IReservationRepository reservationRepository,
        IValidator<ShowtimeRequest> showtimeValidator,
        IValidator<ShowtimeQueryParameters> queryValidator,
        IOptions<CinemaSettings> options)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _hallRepository = hallRepository;
        _reservationRepository = reservationRepository;
        _showtimeValidator = showtimeValidator;
        _queryValidator = queryValidator;
        _cinemaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);
    }

    public async Task<List<MovieShowtimesResponse>> GetByDateAsync(ShowtimeQueryParameters query)
    {
        await _queryValidator.ValidateAndThrowAsync(query);

        var localDayStart = query.Date!.Value.ToDateTime(TimeOnly.MinValue);
        var from = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDayStart, _cinemaTimeZone));
        var to = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDayStart.AddDays(1), _cinemaTimeZone));

        var showtimes = await _showtimeRepository.GetByStartTimeRangeAsync(from, to);

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

    public async Task<ShowtimeResponse> GetByIdAsync(int id)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Showtime with id {id} was not found.");
        return showtime.ToResponse();
    }

    public async Task<SeatMapResponse> GetSeatMapAsync(int id)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Showtime with id {id} was not found.");
        var hall = showtime.Hall;
        var reservedSeats = (await _reservationRepository.GetReservedSeatCodesAsync(id)).ToHashSet();

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

        var movie = await _movieRepository.GetByIdAsync(request.MovieId)
            ?? throw new BadRequestException($"Movie with id {request.MovieId} was not found.");
        var hall = await _hallRepository.GetByIdAsync(request.HallId)
            ?? throw new BadRequestException($"Hall with id {request.HallId} was not found.");

        var startTime = request.StartTime.ToUniversalTime();
        var endTime = startTime.AddMinutes(movie.DurationMinutes);

        if (await _showtimeRepository.HasOverlapAsync(hall.Id, startTime, endTime))
            throw new ConflictException($"Hall '{hall.Name}' already has a showtime overlapping {startTime:u} - {endTime:u}.");

        var showtime = new Showtime
        {
            Movie = movie,
            Hall = hall,
            StartTime = startTime,
            EndTime = endTime,
            TicketPrice = request.TicketPrice,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _showtimeRepository.AddAsync(showtime);
        await _showtimeRepository.SaveChangesAsync();

        return showtime.ToResponse();
    }
}
