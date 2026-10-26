using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces;
using BusinessLayer.Mappings;
using DataAccessLayer.Repositories.Interfaces;
using DtoLayer.Users;
using EntityLayer.Entities;
using EntityLayer.Enums;
using FluentValidation;

namespace BusinessLayer.Concrete;

public class UserManager : IUserService
{
    private const string MainAdminEmail = "admin@cinema.com";

    private readonly IUserRepository _userRepository;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;
    private readonly IValidator<UpdateUserRoleRequest> _updateRoleValidator;

    public UserManager(
        IUserRepository userRepository,
        IValidator<UpdateProfileRequest> updateProfileValidator,
        IValidator<UpdateUserRoleRequest> updateRoleValidator)
    {
        _userRepository = userRepository;
        _updateProfileValidator = updateProfileValidator;
        _updateRoleValidator = updateRoleValidator;
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await GetExistingUserAsync(id);
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

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => u.ToResponse()).ToList();
    }

    public async Task<UserResponse> UpdateRoleAsync(int currentUserId, int id, UpdateUserRoleRequest request)
    {
        await _updateRoleValidator.ValidateAndThrowAsync(request);

        var user = await GetExistingUserAsync(id);

        if (user.Id == currentUserId)
            throw new ForbiddenException("You cannot change your own role.");

        if (user.Email == MainAdminEmail)
            throw new ForbiddenException("The role of the main admin account cannot be changed.");

        var newRole = Enum.Parse<UserRole>(request.Role, ignoreCase: true);

        if (user.Role == newRole)
            throw new ConflictException($"User with id {id} already has the '{newRole}' role.");

        user.Role = newRole;
        await _userRepository.SaveChangesAsync();

        return user.ToResponse();
    }

    private async Task<User> GetExistingUserAsync(int id) =>
        await _userRepository.GetByIdAsync(id)
        ?? throw new NotFoundException($"User with id {id} was not found.");
}
