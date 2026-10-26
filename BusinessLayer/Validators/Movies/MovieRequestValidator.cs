using DtoLayer.Movies;
using FluentValidation;

namespace BusinessLayer.Validators.Movies;

public class MovieRequestValidator : AbstractValidator<MovieRequest>
{
    public MovieRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PosterUrl).NotEmpty().MaximumLength(500).HttpUrl();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 600);
        RuleFor(x => x.ReleaseDate).NotEmpty();
        RuleFor(x => x.GenreIds).NotEmpty().WithMessage("At least one genre must be selected.");
        RuleForEach(x => x.GenreIds).GreaterThan(0);
    }
}
