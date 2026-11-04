using DataAccessLayer.Context;
using EntityLayer.Entities;
using EntityLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, string adminEmail, string? adminPassword)
    {
        await context.Database.MigrateAsync();

        // The admin is looked up by email, so restarts never create a second account.
        // If several instances start at once, the unique email index rejects the duplicate insert.
        var email = adminEmail.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(u => u.Email == email)) return;

        if (string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Admin:Password must be configured to create the initial admin account.");

        context.Users.Add(new User
        {
            FirstName = "System",
            LastName = "Admin",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRole.Admin,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();
    }
}
