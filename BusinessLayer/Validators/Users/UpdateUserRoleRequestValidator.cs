using DtoLayer.Users;
using EntityLayer.Enums;
using FluentValidation;

namespace BusinessLayer.Validators.Users;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .IsEnumName(typeof(UserRole), caseSensitive: false)
            .WithMessage($"Role must be one of: {string.Join(", ", Enum.GetNames<UserRole>())}.");
    }
}
