using DtoLayer.Reservations;
using EntityLayer.Enums;
using FluentValidation;

namespace BusinessLayer.Validators.Reservations;

public class ReservationStatusRequestValidator : AbstractValidator<ReservationStatusRequest>
{
    public ReservationStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => string.Equals(status, nameof(ReservationStatus.Cancelled), StringComparison.OrdinalIgnoreCase))
            .WithMessage($"Status can only be changed to '{nameof(ReservationStatus.Cancelled)}'.");
    }
}
