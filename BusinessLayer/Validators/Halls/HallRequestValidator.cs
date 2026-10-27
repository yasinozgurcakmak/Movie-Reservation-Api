using DtoLayer.Halls;
using FluentValidation;

namespace BusinessLayer.Validators.Halls;

public class HallRequestValidator : AbstractValidator<HallRequest>
{
    public HallRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RowCount).InclusiveBetween(1, 26).WithMessage("Row count must be between 1 and 26 (rows are labeled A-Z).");
        RuleFor(x => x.SeatsPerRow).InclusiveBetween(1, 50);
    }
}
