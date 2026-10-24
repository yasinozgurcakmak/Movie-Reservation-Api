using System.Net;
using System.Security.Cryptography;
using System.Text;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Options;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Auth;
using EntityLayer.Entities;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Concrete;

public class PasswordResetManager : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IEmailService _emailService;
    private readonly PasswordResetSettings _settings;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

    public PasswordResetManager(
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IEmailService emailService,
        IOptions<PasswordResetSettings> options,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _settings = options.Value;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await _forgotPasswordValidator.ValidateAndThrowAsync(request);

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user is null)
            return;

        await _tokenRepository.DeleteByUserIdAsync(user.Id);

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;

        await _tokenRepository.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            ExpiresAt = now.AddMinutes(_settings.ExpirationMinutes),
            CreatedAt = now
        });
        await _tokenRepository.SaveChangesAsync();

        var resetLink = $"{_settings.ResetUrl}?token={rawToken}";
        var body = $"""
            <p>Merhaba, {WebUtility.HtmlEncode(user.FirstName)},</p>
            <p>Şifrenizi sıfırlama talebi aldık. Yeni bir şifre seçmek için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href="{resetLink}">Şifremi sıfırla</a></p>
            <p>Bu bağlantı {_settings.ExpirationMinutes} dakika boyunca geçerlidir ve yalnızca bir kez kullanılabilir.</p>
            <p>Bunu siz talep etmediyseniz, bu e-postayı rahatlıkla göz ardı edebilirsiniz.</p>
            """;

        await _emailService.SendAsync(user.Email, "Reset your password", body);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        await _resetPasswordValidator.ValidateAndThrowAsync(request);

        var token = await _tokenRepository.GetByTokenHashAsync(HashToken(request.Token));

        if (token is null || token.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new BadRequestException("Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.");

        await _tokenRepository.DeleteByUserIdAsync(token.UserId);

        token.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.SaveChangesAsync();
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
