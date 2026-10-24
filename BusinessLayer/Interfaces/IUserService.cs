using DtoLayer.Users;

namespace BusinessLayer.Interfaces;

public interface IUserService
{
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> UpdateProfileAsync(int id, UpdateProfileRequest request);
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse> UpdateRoleAsync(int currentUserId, int id, UpdateUserRoleRequest request);
}
