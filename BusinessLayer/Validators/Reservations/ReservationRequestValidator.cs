using BusinessLayer.Common;
using DtoLayer.Reservations;
using FluentValidation;

namespace BusinessLayer.Validators.Reservations;

public class ReservationRequestValidator : AbstractValidator<ReservationRequest>
{
    public ReservationRequestValidator()
    {
        RuleFor(x => x.ShowtimeId).GreaterThan(0);
        RuleFor(x => x.SeatCodes).NotEmpty().WithMessage("At least one seat must be selected.");
        RuleFor(x => x.SeatCodes)
            .Must(codes => codes.Count <= ReservationLimits.MaxSeatsPerUserPerShowtime)
            .WithMessage($"At most {ReservationLimits.MaxSeatsPerUserPerShowtime} seats can be reserved per showtime.")
            .When(x => x.SeatCodes is not null);
        RuleFor(x => x.SeatCodes)
            .Must(codes => codes.Select(c => c?.Trim().ToUpperInvariant()).Distinct().Count() == codes.Count)
            .WithMessage("Seat codes must be unique.")
            .When(x => x.SeatCodes is not null);
        RuleForEach(x => x.SeatCodes)
            .NotEmpty()
            .Matches("^\\s*[A-Za-z][1-9][0-9]?\\s*$")
            .WithMessage("'{PropertyValue}' is not a valid seat code (expected format: A5).");
    }
}
