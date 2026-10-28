namespace DtoLayer.Reports;

public record AnalyticsQueryParameters
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public int Top { get; init; } = 5;
    public string? Format { get; init; }
}
