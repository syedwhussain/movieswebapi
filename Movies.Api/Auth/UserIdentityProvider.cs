namespace Movies.Api.Auth;

public class UserIdentityProvider : IUserIdentityProvider
{
    public Guid GetUserId(HttpContext httpContext)
    {
        //get current context
        var userIdClaim = httpContext.User.Claims.SingleOrDefault(c => c.Type == "userid");
        var userId = userIdClaim != null ? new Guid(userIdClaim.Value) : Guid.Empty;
        return userId;
    }
}