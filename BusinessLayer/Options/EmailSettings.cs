namespace BusinessLayer.Options;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = null!;
    public string FromName { get; set; } = null!;
}
