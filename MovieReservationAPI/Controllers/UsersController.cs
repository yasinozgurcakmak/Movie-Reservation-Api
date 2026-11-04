using BusinessLayer.Interfaces;
using DtoLayer.Auth;
using DtoLayer.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovieReservationAPI.Extensions;
using MovieReservationAPI.RateLimiting;

namespace MovieReservationAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(User.GetUserId(), cancellationToken);
        return Ok(user);
    }

    [HttpPut("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
    {
        var user = await _userService.UpdateProfileAsync(User.GetUserId(), request);
        return Ok(user);
    }

    // Signs out every other session; the response carries a new token for the current one.
    // Rate limited so a stolen token cannot be used to guess the current password.
    [HttpPut("me/password")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var token = await _userService.ChangePasswordAsync(User.GetUserId(), request);
        return Ok(token);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<List<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPatch("{id:int}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleRequest request)
    {
        var user = await _userService.UpdateRoleAsync(User.GetUserId(), id, request);
        return Ok(user);
    }
}
