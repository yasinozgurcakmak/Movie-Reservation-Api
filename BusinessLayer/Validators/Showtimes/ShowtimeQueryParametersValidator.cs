using DtoLayer.Showtimes;
using FluentValidation;

namespace BusinessLayer.Validators.Showtimes;

public class ShowtimeQueryParametersValidator : AbstractValidator<ShowtimeQueryParameters>
{
    public ShowtimeQueryParametersValidator()
    {
        RuleFor(x => x.Date).NotNull().WithMessage("Date is required (format: YYYY-MM-DD).");
    }
}
