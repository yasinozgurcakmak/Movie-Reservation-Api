namespace DtoLayer.Users;

public record UpdateProfileRequest(string FirstName, string LastName, string? ProfilePictureUrl);
