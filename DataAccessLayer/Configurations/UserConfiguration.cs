using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.EmailConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(32).HasDefaultValueSql("replace(gen_random_uuid()::text, '-', '')");
        builder.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
