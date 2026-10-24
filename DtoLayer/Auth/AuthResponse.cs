namespace DtoLayer.Auth;

public record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt);
