using BusinessLayer.Common;
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using BusinessLayer.Options;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Auth;
using DtoLayer.Users;
using EntityLayer.Entities;
using EntityLayer.Enums;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Concrete;

public class UserManager : IUserService
{
    // Bounds how long a revoked token keeps working if the cache entry could not be removed.
    private static readonly TimeSpan SecurityStampCacheDuration = TimeSpan.FromMinutes(5);

    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserManager> _logger;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly IValidator<UpdateUserRoleRequest> _updateRoleValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly string _mainAdminEmail;

    public UserManager(
        IUserRepository userRepository,
        ITokenService tokenService,
        IDistributedCache cache,
        ILogger<UserManager> logger,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        IValidator<UpdateUserRoleRequest> updateRoleValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator,
        IOptions<AdminSettings> adminOptions)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _cache = cache;
        _logger = logger;
        _updateProfileValidator = updateProfileValidator;
        _updateRoleValidator = updateRoleValidator;
        _changePasswordValidator = changePasswordValidator;
        _mainAdminEmail = adminOptions.Value.Email.Trim().ToLowerInvariant();
    }

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, cancellationToken);
        return user.ToResponse();
    }

    public async Task<UserResponse> UpdateProfileAsync(int id, UpdateProfileRequest request)
    {
        await _updateProfileValidator.ValidateAndThrowAsync(request);

        var user = await GetExistingUserAsync(id);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.ProfilePictureUrl = string.IsNullOrWhiteSpace(request.ProfilePictureUrl)
            ? null
            : request.ProfilePictureUrl.Trim();

        await _userRepository.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<AuthResponse> ChangePasswordAsync(int id, ChangePasswordRequest request)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(request);

        var user = await GetExistingUserAsync(id);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await RotateSecurityStampAsync(user);

        // Every other session is signed out by the new stamp, so the caller gets a fresh token to stay signed in.
        return _tokenService.CreateToken(user);
    }

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => u.ToResponse()).ToList();
    }

    public async Task<UserResponse> UpdateRoleAsync(int currentUserId, int id, UpdateUserRoleRequest request)
    {
        await _updateRoleValidator.ValidateAndThrowAsync(request);

        var user = await GetExistingUserAsync(id);

        if (user.Id == currentUserId)
            throw new ForbiddenException("You cannot change your own role.");

        if (user.Email == _mainAdminEmail)
            throw new ForbiddenException("The role of the main admin account cannot be changed.");

        var newRole = Enum.Parse<UserRole>(request.Role, ignoreCase: true);

        if (user.Role == newRole)
            throw new ConflictException($"User with id {id} already has the '{newRole}' role.");

        // Tokens carry the role, so tokens issued with the old role are revoked.
        user.Role = newRole;
        await RotateSecurityStampAsync(user);

        return user.ToResponse();
    }

    public async Task<bool> IsSecurityStampValidAsync(int userId, string securityStamp)
    {
        var currentStamp = await _cache.GetOrCreateAsync(
            SecurityStamps.CacheKey(userId),
            SecurityStampCacheDuration,
            () => _userRepository.GetSecurityStampAsync(userId),
            _logger);

        return currentStamp == securityStamp;
    }

    private async Task RotateSecurityStampAsync(User user)
    {
        user.SecurityStamp = SecurityStamps.New();
        await _userRepository.SaveChangesAsync();
        await _cache.TryRemoveAsync(SecurityStamps.CacheKey(user.Id), _logger);
    }

    private async Task<User> GetExistingUserAsync(int id, CancellationToken cancellationToken = default) =>
        await _userRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"User with id {id} was not found.");
}
