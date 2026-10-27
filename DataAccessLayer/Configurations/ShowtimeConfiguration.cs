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
        builder.HasIndex(s => new { s.HallId, s.StartTime });
        builder.HasIndex(s => s.StartTime);
        builder.HasQueryFilter(s => !s.Movie.IsDeleted);
        builder.HasOne(s => s.Movie).WithMany().HasForeignKey(s => s.MovieId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Hall).WithMany().HasForeignKey(s => s.HallId).OnDelete(DeleteBehavior.Restrict);
    }
}
