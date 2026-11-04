using BusinessLayer.Interfaces;
using DtoLayer.Common;
using DtoLayer.Reports;
using DtoLayer.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieReservationAPI.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class AdminController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IReportService _reportService;

    public AdminController(IReservationService reservationService, IReportService reportService)
    {
        _reservationService = reservationService;
        _reportService = reportService;
    }

    [HttpGet("reservations")]
    [ProducesResponseType<PagedResponse<AdminReservationResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReservations([FromQuery] ReservationQueryParameters query, CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetPagedAsync(query, cancellationToken);
        return Ok(reservations);
    }

    [HttpGet("reports/analytics")]
    [ProducesResponseType<AnalyticsResponse>(StatusCodes.Status200OK, "application/json", "text/csv")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAnalytics([FromQuery] AnalyticsQueryParameters query, CancellationToken cancellationToken)
    {
        if (string.Equals(query.Format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = await _reportService.GetAnalyticsCsvAsync(query, cancellationToken);
            return File(csv, "text/csv", "analytics.csv");
        }

        var analytics = await _reportService.GetAnalyticsAsync(query, cancellationToken);
        return Ok(analytics);
    }

    [HttpGet("reports/sales")]
    [ProducesResponseType<SalesReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSales([FromQuery] SalesQueryParameters query, CancellationToken cancellationToken)
    {
        var sales = await _reportService.GetSalesAsync(query, cancellationToken);
        return Ok(sales);
    }
}
