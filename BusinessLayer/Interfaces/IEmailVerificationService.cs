using DtoLayer.Auth;
using EntityLayer.Entities;

namespace BusinessLayer.Interfaces;

public interface IEmailVerificationService
{
    Task SendVerificationEmailAsync(User user);
    Task VerifyEmailAsync(VerifyEmailRequest request);
    Task ResendVerificationEmailAsync(ResendVerificationRequest request);
}
