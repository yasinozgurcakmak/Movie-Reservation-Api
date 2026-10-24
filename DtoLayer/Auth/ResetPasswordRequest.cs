namespace DtoLayer.Auth;

public record ResetPasswordRequest(string Token, string NewPassword);
