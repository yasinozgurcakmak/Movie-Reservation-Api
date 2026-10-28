namespace DataAccessLayer.Models;

public record ShowtimeSalesSummary(
    int ShowtimeId,
    int MovieId,
    string MovieTitle,
    int HallId,
    string HallName,
    DateTimeOffset StartTime,
    int Capacity,
    int SoldSeats,
    decimal Revenue,
    int CancelledSeats,
    decimal LostRevenue);
