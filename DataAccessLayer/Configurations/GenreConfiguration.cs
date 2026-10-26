using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("Genres");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(g => g.Name).IsUnique();

        builder.HasData( new Genre { Id = 1, Name = "Aksiyon" }, new Genre { Id = 2, Name = "Komedi" },  new Genre { Id = 3, Name = "Dram" }, new Genre { Id = 4, Name = "Korku" }, new Genre { Id = 5, Name = "Bilim Kurgu" }, new Genre { Id = 6, Name = "Animasyon" }, new Genre { Id = 7, Name = "Gerilim" }, new Genre { Id = 8, Name = "Romantik" });
    }
}
