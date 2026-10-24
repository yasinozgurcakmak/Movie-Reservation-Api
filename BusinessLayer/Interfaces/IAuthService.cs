using DtoLayer.Auth;
using DtoLayer.Users;

namespace BusinessLayer.Interfaces;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
