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
    public async Task<IActionResult> GetByDate([FromQuery] ShowtimeQueryParameters query)
    {
        var movies = await _showtimeService.GetByDateAsync(query);
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ShowtimeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var showtime = await _showtimeService.GetByIdAsync(id);
        return Ok(showtime);
    }

    [HttpGet("{id:int}/seats")]
    [ProducesResponseType<SeatMapResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeatMap(int id)
    {
        var seatMap = await _showtimeService.GetSeatMapAsync(id);
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
}
