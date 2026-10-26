using DtoLayer.Genres;
using FluentValidation;

namespace BusinessLayer.Validators.Genres;

public class GenreRequestValidator : AbstractValidator<GenreRequest>
{
    public GenreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
