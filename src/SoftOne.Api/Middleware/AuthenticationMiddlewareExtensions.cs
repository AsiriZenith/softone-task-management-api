namespace SoftOne.Api.Middleware;

internal static class AuthenticationMiddlewareExtensions
{
    public static bool RequiresAuthentication(HttpContext context)
    {
        var path = context.Request.Path;

        if (!path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.StartsWithSegments("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            && HttpMethods.IsPost(context.Request.Method))
        {
            return false;
        }

        return true;
    }
}
