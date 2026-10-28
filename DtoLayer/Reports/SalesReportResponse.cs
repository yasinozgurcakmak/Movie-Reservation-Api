namespace DtoLayer.Reports;

public record SalesReportResponse(
    DateOnly From,
    DateOnly To,
    string Interval,
    int ReservationCount,
    int TicketsSold,
    decimal Revenue,
    List<SalesPointResponse> Points);

public record SalesPointResponse(DateOnly PeriodStart, int ReservationCount, int TicketsSold, decimal Revenue);
