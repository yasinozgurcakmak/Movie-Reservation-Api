namespace DtoLayer.Users;

public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? ProfilePictureUrl,
    string Role,
    DateTimeOffset CreatedAt);
