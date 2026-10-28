using DtoLayer.Reports;
using FluentValidation;

namespace BusinessLayer.Validators.Reports;

public class SalesQueryParametersValidator : AbstractValidator<SalesQueryParameters>
{
    private const int MaxRangeDays = 366;
    private static readonly string[] Intervals = ["day", "week"];

    public SalesQueryParametersValidator()
    {
        RuleFor(x => x.From).NotNull().WithMessage("'From' is required (format: YYYY-MM-DD).");
        RuleFor(x => x.To).NotNull().WithMessage("'To' is required (format: YYYY-MM-DD).");
        RuleFor(x => x.To)
            .Must((query, to) => to!.Value >= query.From!.Value && to.Value.DayNumber - query.From.Value.DayNumber < MaxRangeDays)
            .WithMessage($"'To' must be on or after 'From' and the range cannot exceed {MaxRangeDays} days.")
            .When(x => x.From.HasValue && x.To.HasValue);
        RuleFor(x => x.Interval)
            .Must(i => Intervals.Contains(i, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Interval must be 'day' or 'week'.");
    }
}
