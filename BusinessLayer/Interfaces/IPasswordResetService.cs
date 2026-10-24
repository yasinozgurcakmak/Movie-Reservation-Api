using DtoLayer.Auth;

namespace BusinessLayer.Interfaces;

public interface IPasswordResetService
{
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
