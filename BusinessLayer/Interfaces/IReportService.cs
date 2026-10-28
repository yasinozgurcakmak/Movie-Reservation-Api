using DtoLayer.Reports;

namespace BusinessLayer.Interfaces;

public interface IReportService
{
    Task<AnalyticsResponse> GetAnalyticsAsync(AnalyticsQueryParameters query);
    Task<byte[]> GetAnalyticsCsvAsync(AnalyticsQueryParameters query);
    Task<SalesReportResponse> GetSalesAsync(SalesQueryParameters query);
}
