using BusinessLayer.Interfaces;
using DtoLayer.Auth;
using DtoLayer.Common;
using DtoLayer.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovieReservationAPI.RateLimiting;

namespace MovieReservationAPI.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitPolicies.Auth)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailVerificationService _emailVerificationService;

    public AuthController(
        IAuthService authService,
        IPasswordResetService passwordResetService,
        IEmailVerificationService emailVerificationService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
        _emailVerificationService = emailVerificationService;
    }

    [HttpPost("verify-email")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        await _emailVerificationService.VerifyEmailAsync(request);
        return Ok(new MessageResponse("Your email address has been verified."));
    }

    [HttpPost("resend-verification")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification(ResendVerificationRequest request)
    {
        await _emailVerificationService.ResendVerificationEmailAsync(request);
        return Ok(new MessageResponse("If an unverified account with that email exists, a verification link has been sent."));
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await _passwordResetService.ForgotPasswordAsync(request);
        return Ok(new MessageResponse("If an account with that email exists, a password reset link has been sent."));
    }

    [HttpPost("reset-password")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        await _passwordResetService.ResetPasswordAsync(request);
        return Ok(new MessageResponse("Your password has been reset successfully."));
    }

    [HttpPost("register")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);
        return Ok(token);
    }
}
