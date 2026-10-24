using DtoLayer.Auth;
using EntityLayer.Entities;

namespace BusinessLayer.Interfaces;

public interface ITokenService
{
    AuthResponse CreateToken(User user);
}
