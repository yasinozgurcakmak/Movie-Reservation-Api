using BusinessLayer.Interfaces;
using DtoLayer.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieReservationAPI.Extensions;

namespace MovieReservationAPI.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ReservationRequest request)
    {
        var reservation = await _reservationService.CreateAsync(User.GetUserId(), request);
        return StatusCode(StatusCodes.Status201Created, reservation);
    }

    [HttpGet("my")]
    [ProducesResponseType<List<ReservationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMy()
    {
        var reservations = await _reservationService.GetByUserIdAsync(User.GetUserId());
        return Ok(reservations);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(int id, ReservationStatusRequest request)
    {
        var reservation = await _reservationService.UpdateStatusAsync(User.GetUserId(), id, request);
        return Ok(reservation);
    }
}
