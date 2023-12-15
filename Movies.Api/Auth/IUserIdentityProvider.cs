namespace Movies.Api.Auth;

public interface IUserIdentityProvider
{
    public Guid GetUserId(HttpContext httpContext);
}