namespace DtoLayer.Users;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
