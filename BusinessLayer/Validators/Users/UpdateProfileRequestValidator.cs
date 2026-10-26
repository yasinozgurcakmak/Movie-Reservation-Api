using DtoLayer.Users;
using FluentValidation;

namespace BusinessLayer.Validators.Users;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

        RuleFor(x => x.ProfilePictureUrl)
            .MaximumLength(500)
            .HttpUrl()
            .When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));
    }
}
