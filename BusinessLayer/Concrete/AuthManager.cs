using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Auth;
using DtoLayer.Users;
using EntityLayer.Entities;
using EntityLayer.Enums;
using FluentValidation;

namespace BusinessLayer.Concrete;

public class AuthManager : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthManager(
        IUserRepository userRepository,
        ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        await _registerValidator.ValidateAndThrowAsync(request);

        var email = NormalizeEmail(request.Email);

        if (await _userRepository.EmailExistsAsync(email))
            throw new ConflictException($"A user with email '{email}' already exists.");

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        await _loginValidator.ValidateAndThrowAsync(request);

        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email));

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) throw new UnauthorizedException("Invalid email or password.");

        return _tokenService.CreateToken(user);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
