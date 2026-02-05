using System.Security.Claims;
using LocalEcho.Application.Interfaces;

namespace LocalEcho.API.Services;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) 
                throw new UnauthorizedAccessException("Context is not available");

            var idString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(idString) || !Guid.TryParse(idString, out var userId))
                throw new UnauthorizedAccessException("User ID claim is missing or invalid");

            return userId;
        }
    }

    public Guid DistrictId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) 
                throw new UnauthorizedAccessException("Context is not available");

            var districtStr = user.FindFirst("DistrictId")?.Value;

            if (string.IsNullOrEmpty(districtStr) || !Guid.TryParse(districtStr, out var districtId))
                throw new UnauthorizedAccessException("District ID claim is missing or invalid");

            return districtId;
        }
    }
}