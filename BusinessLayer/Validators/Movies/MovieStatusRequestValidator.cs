using DtoLayer.Movies;
using FluentValidation;

namespace BusinessLayer.Validators.Movies;

public class MovieStatusRequestValidator : AbstractValidator<MovieStatusRequest>
{
    public MovieStatusRequestValidator()
    {
        RuleFor(x => x.IsDeleted).NotNull();
    }
}
