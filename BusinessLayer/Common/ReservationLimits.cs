namespace BusinessLayer.Common;

public static class ReservationLimits
{
    // Without payment a single account could otherwise hold every seat of a showtime.
    public const int MaxSeatsPerUserPerShowtime = 10;
}
