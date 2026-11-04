using EntityLayer.Enums;

namespace EntityLayer.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool EmailConfirmed { get; set; }
    // Rotated whenever credentials or role change, which invalidates every token issued before.
    public string SecurityStamp { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
