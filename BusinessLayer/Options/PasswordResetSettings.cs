namespace BusinessLayer.Options;

public class PasswordResetSettings
{
    public const string SectionName = "PasswordReset";
    public string ResetUrl { get; set; } = null!;
    public int ExpirationMinutes { get; set; }
}
