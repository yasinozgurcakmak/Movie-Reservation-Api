using DtoLayer.Auth;
using FluentValidation;

namespace BusinessLayer.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        // BCrypt only uses the first 72 bytes, so longer input is never a valid password.
        RuleFor(x => x.Password).NotEmpty().MaximumLength(72);
    }
}
