using DataAccessLayer.Auditing;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MovieReservationAPI.Auditing;

public class HttpAuditUserAccessor : IAuditUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpAuditUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId =>
        int.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;
}
