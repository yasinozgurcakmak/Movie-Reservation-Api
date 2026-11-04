using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Changes).IsRequired().HasColumnType("jsonb");
        builder.Property(a => a.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.CreatedAt);
    }
}
