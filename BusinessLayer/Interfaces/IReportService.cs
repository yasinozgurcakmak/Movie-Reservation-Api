using DtoLayer.Reports;

namespace BusinessLayer.Interfaces;

public interface IReportService
{
    Task<AnalyticsResponse> GetAnalyticsAsync(AnalyticsQueryParameters query, CancellationToken cancellationToken = default);
    Task<byte[]> GetAnalyticsCsvAsync(AnalyticsQueryParameters query, CancellationToken cancellationToken = default);
    Task<SalesReportResponse> GetSalesAsync(SalesQueryParameters query, CancellationToken cancellationToken = default);
}
