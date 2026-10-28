using DtoLayer.Reports;
using FluentValidation;

namespace BusinessLayer.Validators.Reports;

public class AnalyticsQueryParametersValidator : AbstractValidator<AnalyticsQueryParameters>
{
    private static readonly string[] Formats = ["json", "csv"];

    public AnalyticsQueryParametersValidator()
    {
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("'To' must be on or after 'From'.")
            .When(x => x.From.HasValue && x.To.HasValue);
        RuleFor(x => x.Top).InclusiveBetween(1, 50);
        RuleFor(x => x.Format)
            .Must(f => Formats.Contains(f, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Format must be 'json' or 'csv'.")
            .When(x => x.Format is not null);
    }
}
