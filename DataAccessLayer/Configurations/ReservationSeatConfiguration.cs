using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class ReservationSeatConfiguration : IEntityTypeConfiguration<ReservationSeat>
{
    public void Configure(EntityTypeBuilder<ReservationSeat> builder)
    {
        builder.ToTable("ReservationSeats");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SeatCode).IsRequired().HasMaxLength(3);
        builder.Property(s => s.IsCancelled).IsRequired().HasDefaultValue(false);
        builder.HasOne<Showtime>().WithMany().HasForeignKey(s => s.ShowtimeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(s => new { s.ShowtimeId, s.SeatCode }).IsUnique().HasFilter("\"IsCancelled\" = false");
    }
}
