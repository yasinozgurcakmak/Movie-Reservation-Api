namespace DtoLayer.Reservations;

public record ReservationQueryParameters
{
    public string? Status { get; init; }
    public int? ShowtimeId { get; init; }
    public int? UserId { get; init; }
    public string? Email { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
