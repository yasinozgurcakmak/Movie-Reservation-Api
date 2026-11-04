namespace DtoLayer.Users;

public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    bool EmailConfirmed,
    string? ProfilePictureUrl,
    string Role,
    DateTimeOffset CreatedAt);
