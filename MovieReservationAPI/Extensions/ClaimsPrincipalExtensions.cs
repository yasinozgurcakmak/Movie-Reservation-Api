using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MovieReservationAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
