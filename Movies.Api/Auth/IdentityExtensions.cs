namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var userId = "d8566de3-b1a6-4a9b-b842-8e3887a82e41";//context.User.Claims.SingleOrDefault(x => x.Type == "userid");

        if (Guid.TryParse(userId, out var parsedId))
        {
            return parsedId;
        }

        return null;
    }
}
