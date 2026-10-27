using BusinessLayer.Interfaces;
using DtoLayer.Halls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieReservationAPI.Controllers;

[ApiController]
[Route("api/halls")]
public class HallsController : ControllerBase
{
    private const string ContentManagerRoles = "Admin,Editor";

    private readonly IHallService _hallService;

    public HallsController(IHallService hallService)
    {
        _hallService = hallService;
    }

    [HttpGet]
    [ProducesResponseType<List<HallResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var halls = await _hallService.GetAllAsync();
        return Ok(halls);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<HallResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var hall = await _hallService.GetByIdAsync(id);
        return Ok(hall);
    }

    [HttpPost]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType<HallResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(HallRequest request)
    {
        var hall = await _hallService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = hall.Id }, hall);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType<HallResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, HallRequest request)
    {
        var hall = await _hallService.UpdateAsync(id, request);
        return Ok(hall);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _hallService.DeleteAsync(id);
        return NoContent();
    }
}
