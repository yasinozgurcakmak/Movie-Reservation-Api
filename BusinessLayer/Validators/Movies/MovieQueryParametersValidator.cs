using DtoLayer.Movies;
using FluentValidation;

namespace BusinessLayer.Validators.Movies;

public class MovieQueryParametersValidator : AbstractValidator<MovieQueryParameters>
{
    public MovieQueryParametersValidator()
    {
        RuleFor(x => x.GenreId).GreaterThan(0).When(x => x.GenreId.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
