using System.Security.Claims;
using UserGroupSite.Data.Interfaces;

namespace UserGroupSite.Server.Services;

public class HttpUserService : IUserService
{
    private IHttpContextAccessor HttpContextAccessor { get; }

    /// <summary>Initializes a new instance of the <see cref="HttpUserService"/> class.</summary>
    /// <param name="httpContextAccessor">The accessor for the current HTTP context.</param>
    public HttpUserService(IHttpContextAccessor httpContextAccessor)
    {
        HttpContextAccessor = httpContextAccessor;
    }

    /// <summary>Gets the current authenticated user's ID.</summary>
    public int UserId
    {
        get
        {
            return Convert.ToInt32(HttpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}