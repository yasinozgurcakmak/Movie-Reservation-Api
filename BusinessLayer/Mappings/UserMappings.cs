using DtoLayer.Users;
using EntityLayer.Entities;

namespace BusinessLayer.Mappings;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.ProfilePictureUrl,
        user.Role.ToString(),
        user.CreatedAt);
}
