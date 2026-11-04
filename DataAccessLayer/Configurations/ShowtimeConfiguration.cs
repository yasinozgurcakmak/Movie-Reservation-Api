using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class ShowtimeConfiguration : IEntityTypeConfiguration<Showtime>
{
    public void Configure(EntityTypeBuilder<Showtime> builder)
    {
        builder.ToTable("Showtimes", t =>
        {
            t.HasCheckConstraint("CK_Showtimes_EndTime", "\"EndTime\" > \"StartTime\"");
            t.HasCheckConstraint("CK_Showtimes_TicketPrice", "\"TicketPrice\" > 0");
        });
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();
        builder.Property(s => s.TicketPrice).IsRequired().HasPrecision(10, 2);
        builder.Property(s => s.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(s => s.IsCancelled).IsRequired().HasDefaultValue(false);
        builder.HasIndex(s => new { s.HallId, s.StartTime });
        builder.HasIndex(s => s.StartTime);
        // Overlapping showtimes in the same hall are also rejected by the "EX_Showtimes_HallId_TimeRange"
        // exclusion constraint, which is created with raw SQL in the migration (EF Core has no Fluent API for it).
        builder.HasQueryFilter(s => !s.Movie.IsDeleted && !s.IsCancelled);
        builder.HasOne(s => s.Movie).WithMany().HasForeignKey(s => s.MovieId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Hall).WithMany().HasForeignKey(s => s.HallId).OnDelete(DeleteBehavior.Restrict);
    }
}
