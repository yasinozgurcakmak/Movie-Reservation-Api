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
            .Must(BeAValidHttpUrl).WithMessage("Profile picture URL must be a valid http or https URL.")
            .When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl));
    }

    private static bool BeAValidHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
