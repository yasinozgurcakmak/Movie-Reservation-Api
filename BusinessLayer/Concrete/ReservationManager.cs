using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Common;
using DtoLayer.Reservations;
using EntityLayer.Entities;
using EntityLayer.Enums;
using FluentValidation;

namespace BusinessLayer.Concrete;

public class ReservationManager : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IValidator<ReservationRequest> _reservationValidator;
    private readonly IValidator<ReservationStatusRequest> _statusValidator;
    private readonly IValidator<ReservationQueryParameters> _queryValidator;

    public ReservationManager(
        IReservationRepository reservationRepository,
        IShowtimeRepository showtimeRepository,
        IValidator<ReservationRequest> reservationValidator,
        IValidator<ReservationStatusRequest> statusValidator,
        IValidator<ReservationQueryParameters> queryValidator)
    {
        _reservationRepository = reservationRepository;
        _showtimeRepository = showtimeRepository;
        _reservationValidator = reservationValidator;
        _statusValidator = statusValidator;
        _queryValidator = queryValidator;
    }

    public async Task<PagedResponse<AdminReservationResponse>> GetPagedAsync(ReservationQueryParameters query)
    {
        await _queryValidator.ValidateAndThrowAsync(query);

        ReservationStatus? status = query.Status is null ? null : Enum.Parse<ReservationStatus>(query.Status, ignoreCase: true);
        var (reservations, totalCount) = await _reservationRepository.GetPagedAsync(
            status, query.ShowtimeId, query.UserId, query.Email, query.Page, query.PageSize);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResponse<AdminReservationResponse>(
            reservations.Select(r => r.ToAdminResponse()).ToList(),
            query.Page,
            query.PageSize,
            totalCount,
            totalPages);
    }

    public async Task<List<ReservationResponse>> GetByUserIdAsync(int userId)
    {
        var reservations = await _reservationRepository.GetByUserIdAsync(userId);
        return reservations.Select(r => r.ToResponse()).ToList();
    }

    public async Task<ReservationResponse> UpdateStatusAsync(int userId, int id, ReservationStatusRequest request)
    {
        await _statusValidator.ValidateAndThrowAsync(request);

        var reservation = await _reservationRepository.GetByIdForUserAsync(id, userId)
            ?? throw new NotFoundException($"Reservation with id {id} was not found.");

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new ConflictException($"Reservation with id {id} is already cancelled.");

        if (reservation.Showtime.StartTime <= DateTimeOffset.UtcNow)
            throw new BadRequestException("Only reservations for upcoming showtimes can be cancelled.");

        reservation.Status = ReservationStatus.Cancelled;
        reservation.Seats.ForEach(s => s.IsCancelled = true);
        await _reservationRepository.SaveChangesAsync();

        return reservation.ToResponse();
    }

    public async Task<ReservationResponse> CreateAsync(int userId, ReservationRequest request)
    {
        await _reservationValidator.ValidateAndThrowAsync(request);

        var showtime = await _showtimeRepository.GetByIdAsync(request.ShowtimeId)
            ?? throw new BadRequestException($"Showtime with id {request.ShowtimeId} was not found.");

        if (showtime.StartTime <= DateTimeOffset.UtcNow)
            throw new BadRequestException("Seats cannot be reserved for a showtime that has already started.");

        var seatCodes = request.SeatCodes.Select(c => c.Trim().ToUpperInvariant()).ToList();

        var invalidSeats = seatCodes.Where(c => !SeatLayout.Exists(showtime.Hall, c)).ToList();
        if (invalidSeats.Count > 0)
            throw new BadRequestException($"Seats do not exist in hall '{showtime.Hall.Name}': {string.Join(", ", invalidSeats)}.");

        var reservedSeats = await _reservationRepository.GetReservedSeatCodesAsync(showtime.Id);
        var takenSeats = seatCodes.Intersect(reservedSeats).ToList();
        if (takenSeats.Count > 0)
            throw new ConflictException($"Seats are already reserved: {string.Join(", ", takenSeats)}.");

        var reservation = new Reservation
        {
            UserId = userId,
            ShowtimeId = showtime.Id,
            TotalAmount = showtime.TicketPrice * seatCodes.Count,
            Status = ReservationStatus.Confirmed,
            CreatedAt = DateTimeOffset.UtcNow,
            Seats = seatCodes.Select(code => new ReservationSeat { ShowtimeId = showtime.Id, SeatCode = code }).ToList()
        };

        await _reservationRepository.AddAsync(reservation);
        await _reservationRepository.SaveChangesAsync();

        return new ReservationResponse(
            reservation.Id,
            showtime.Id,
            showtime.Movie.Title,
            showtime.Hall.Name,
            showtime.StartTime,
            seatCodes,
            reservation.TotalAmount,
            reservation.Status.ToString(),
            reservation.CreatedAt);
    }
}
