using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies", t => t.HasCheckConstraint("CK_Movies_DurationMinutes", "\"DurationMinutes\" > 0"));
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.PosterUrl).IsRequired().HasMaxLength(500);
        builder.Property(m => m.DurationMinutes).IsRequired();
        builder.Property(m => m.ReleaseDate).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(m => m.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.HasQueryFilter(m => !m.IsDeleted);
        builder.HasMany(m => m.Genres).WithMany().UsingEntity(
            "MovieGenres",
            genre => genre.HasOne(typeof(Genre)).WithMany().HasForeignKey("GenreId").OnDelete(DeleteBehavior.Cascade),
            movie => movie.HasOne(typeof(Movie)).WithMany().HasForeignKey("MovieId").OnDelete(DeleteBehavior.Cascade));
    }
}
