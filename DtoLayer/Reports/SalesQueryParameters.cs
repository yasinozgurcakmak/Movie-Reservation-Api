namespace DtoLayer.Reports;

public record SalesQueryParameters
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public string Interval { get; init; } = "day";
}
