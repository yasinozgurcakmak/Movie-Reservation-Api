using BusinessLayer.Interfaces;
using DtoLayer.Showtimes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieReservationAPI.Controllers;

[ApiController]
[Route("api/showtimes")]
public class ShowtimesController : ControllerBase
{
    private const string ContentManagerRoles = "Admin,Editor";

    private readonly IShowtimeService _showtimeService;

    public ShowtimesController(IShowtimeService showtimeService)
    {
        _showtimeService = showtimeService;
    }

    [HttpGet]
    [ProducesResponseType<List<MovieShowtimesResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByDate([FromQuery] ShowtimeQueryParameters query, CancellationToken cancellationToken)
    {
        var movies = await _showtimeService.GetByDateAsync(query, cancellationToken);
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ShowtimeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var showtime = await _showtimeService.GetByIdAsync(id, cancellationToken);
        return Ok(showtime);
    }

    [HttpGet("{id:int}/seats")]
    [ProducesResponseType<SeatMapResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeatMap(int id, CancellationToken cancellationToken)
    {
        var seatMap = await _showtimeService.GetSeatMapAsync(id, cancellationToken);
        return Ok(seatMap);
    }

    [HttpPost]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType<ShowtimeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ShowtimeRequest request)
    {
        var showtime = await _showtimeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, showtime);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType<ShowtimeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, ShowtimeRequest request)
    {
        var showtime = await _showtimeService.UpdateAsync(id, request);
        return Ok(showtime);
    }

    // Cancels the showtime and every reservation for it; customers are notified by email.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _showtimeService.CancelAsync(id);
        return NoContent();
    }
}
