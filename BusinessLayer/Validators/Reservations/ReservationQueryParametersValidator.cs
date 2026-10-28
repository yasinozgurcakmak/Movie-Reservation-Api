using DtoLayer.Reservations;
using EntityLayer.Enums;
using FluentValidation;

namespace BusinessLayer.Validators.Reservations;

public class ReservationQueryParametersValidator : AbstractValidator<ReservationQueryParameters>
{
    public ReservationQueryParametersValidator()
    {
        RuleFor(x => x.Status)
            .IsEnumName(typeof(ReservationStatus), caseSensitive: false)
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<ReservationStatus>())}.")
            .When(x => x.Status is not null);
        RuleFor(x => x.ShowtimeId).GreaterThan(0).When(x => x.ShowtimeId.HasValue);
        RuleFor(x => x.UserId).GreaterThan(0).When(x => x.UserId.HasValue);
        RuleFor(x => x.Email).MaximumLength(256);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
