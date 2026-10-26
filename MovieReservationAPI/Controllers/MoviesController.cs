using BusinessLayer.Interfaces;
using DtoLayer.Common;
using DtoLayer.Movies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieReservationAPI.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private const string ContentManagerRoles = "Admin,Editor";

    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<MovieResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] MovieQueryParameters query)
    {
        var movies = await _movieService.GetPagedAsync(query);
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        return Ok(movie);
    }

    [HttpPost]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(MovieRequest request)
    {
        var movie = await _movieService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, MovieRequest request)
    {
        var movie = await _movieService.UpdateAsync(id, request);
        return Ok(movie);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _movieService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = ContentManagerRoles)]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(int id, MovieStatusRequest request)
    {
        var movie = await _movieService.UpdateStatusAsync(id, request);
        return Ok(movie);
    }
}
