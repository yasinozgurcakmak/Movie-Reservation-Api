using DtoLayer.Showtimes;
using FluentValidation;

namespace BusinessLayer.Validators.Showtimes;

public class ShowtimeRequestValidator : AbstractValidator<ShowtimeRequest>
{
    public ShowtimeRequestValidator()
    {
        RuleFor(x => x.MovieId).GreaterThan(0);
        RuleFor(x => x.HallId).GreaterThan(0);
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.TicketPrice).GreaterThan(0).PrecisionScale(10, 2, true);
    }
}
