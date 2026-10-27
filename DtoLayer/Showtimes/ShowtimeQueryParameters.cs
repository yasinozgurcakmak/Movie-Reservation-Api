namespace DtoLayer.Showtimes;

public record ShowtimeQueryParameters
{
    public DateOnly? Date { get; init; }
}
