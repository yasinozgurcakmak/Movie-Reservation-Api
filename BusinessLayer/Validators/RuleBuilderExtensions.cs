using FluentValidation;

namespace BusinessLayer.Validators;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(72)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

    public static IRuleBuilderOptions<T, string?> HttpUrl<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("{PropertyName} must be a valid http or https URL.");
}
