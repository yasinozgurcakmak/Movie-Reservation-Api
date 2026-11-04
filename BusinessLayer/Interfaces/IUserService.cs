using DtoLayer.Auth;
using DtoLayer.Users;

namespace BusinessLayer.Interfaces;

public interface IUserService
{
    Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateProfileAsync(int id, UpdateProfileRequest request);
    Task<AuthResponse> ChangePasswordAsync(int id, ChangePasswordRequest request);
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateRoleAsync(int currentUserId, int id, UpdateUserRoleRequest request);
    Task<bool> IsSecurityStampValidAsync(int userId, string securityStamp);
}
