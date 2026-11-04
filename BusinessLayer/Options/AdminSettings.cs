namespace BusinessLayer.Options;

public class AdminSettings
{
    public const string SectionName = "Admin";
    public string Email { get; set; } = null!;
    public string? Password { get; set; }
}
