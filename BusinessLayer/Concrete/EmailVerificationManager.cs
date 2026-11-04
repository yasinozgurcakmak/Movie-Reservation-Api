using System.Net;
using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Options;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Auth;
using EntityLayer.Entities;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Concrete;

public class EmailVerificationManager : IEmailVerificationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly IEmailQueue _emailQueue;
    private readonly EmailVerificationSettings _settings;
    private readonly IValidator<VerifyEmailRequest> _verifyValidator;
    private readonly IValidator<ResendVerificationRequest> _resendValidator;

    public EmailVerificationManager(
        IUserRepository userRepository,
        IEmailVerificationTokenRepository tokenRepository,
        IEmailQueue emailQueue,
        IOptions<EmailVerificationSettings> options,
        IValidator<VerifyEmailRequest> verifyValidator,
        IValidator<ResendVerificationRequest> resendValidator)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _emailQueue = emailQueue;
        _settings = options.Value;
        _verifyValidator = verifyValidator;
        _resendValidator = resendValidator;
    }

    public async Task SendVerificationEmailAsync(User user)
    {
        await _tokenRepository.DeleteByUserIdAsync(user.Id);

        var rawToken = SecureTokens.Generate();
        var now = DateTimeOffset.UtcNow;

        await _tokenRepository.AddAsync(new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = SecureTokens.Hash(rawToken),
            ExpiresAt = now.AddMinutes(_settings.ExpirationMinutes),
            CreatedAt = now
        });
        await _tokenRepository.SaveChangesAsync();

        var verifyLink = $"{_settings.VerifyUrl}?token={rawToken}";
        var body = $"""
            <p>Merhaba, {WebUtility.HtmlEncode(user.FirstName)},</p>
            <p>Hesabınızı etkinleştirmek için e-posta adresinizi doğrulayın:</p>
            <p><a href="{verifyLink}">E-postamı doğrula</a></p>
            <p>Bu bağlantı {_settings.ExpirationMinutes / 60} saat boyunca geçerlidir. Doğrulama yapılmadan rezervasyon oluşturulamaz.</p>
            """;

        _emailQueue.Enqueue(new EmailMessage(user.Email, "Verify your email address", body));
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request)
    {
        await _verifyValidator.ValidateAndThrowAsync(request);

        var token = await _tokenRepository.GetByTokenHashAsync(SecureTokens.Hash(request.Token));

        if (token is null || token.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new BadRequestException("E-posta doğrulama bağlantısı geçersiz veya süresi dolmuş.");

        await _tokenRepository.DeleteByUserIdAsync(token.UserId);

        token.User.EmailConfirmed = true;
        await _userRepository.SaveChangesAsync();
    }

    public async Task ResendVerificationEmailAsync(ResendVerificationRequest request)
    {
        await _resendValidator.ValidateAndThrowAsync(request);

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user is null || user.EmailConfirmed)
            return;

        await SendVerificationEmailAsync(user);
    }
}
