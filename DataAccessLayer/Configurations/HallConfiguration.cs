using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Halls", t =>
        {
            t.HasCheckConstraint("CK_Halls_RowCount", "\"RowCount\" BETWEEN 1 AND 26");
            t.HasCheckConstraint("CK_Halls_SeatsPerRow", "\"SeatsPerRow\" BETWEEN 1 AND 50");
        });
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(h => h.Name).IsUnique();
        builder.Property(h => h.RowCount).IsRequired();
        builder.Property(h => h.SeatsPerRow).IsRequired();
        builder.Property(h => h.Version).IsRowVersion();
    }
}
