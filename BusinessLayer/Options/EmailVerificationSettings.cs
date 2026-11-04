namespace BusinessLayer.Options;

public class EmailVerificationSettings
{
    public const string SectionName = "EmailVerification";
    public string VerifyUrl { get; set; } = null!;
    public int ExpirationMinutes { get; set; }
}
