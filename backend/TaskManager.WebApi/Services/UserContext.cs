using System.Security.Claims;
using TaskManager.WebApi.Exceptions;

namespace TaskManager.WebApi.Services;

public class UserContext(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var str = _httpContextAccessor.HttpContext?.User.FindFirstValue("id");
            
            if(str is null)
                throw new ApiException(401, "User not authorized");
            
            return Guid.Parse(str);
        }
    }
}
