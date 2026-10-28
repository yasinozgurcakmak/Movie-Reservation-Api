using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", t => t.HasCheckConstraint("CK_Reservations_TotalAmount", "\"TotalAmount\" > 0"));
        builder.HasKey(r => r.Id);
        builder.Property(r => r.TotalAmount).IsRequired().HasPrecision(10, 2);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Showtime).WithMany().HasForeignKey(r => r.ShowtimeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.Seats).WithOne(s => s.Reservation).HasForeignKey(s => s.ReservationId).OnDelete(DeleteBehavior.Cascade);
    }
}
