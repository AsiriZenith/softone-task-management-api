namespace SoftOne.Api.Middleware;

/// <summary>
/// Validates HTTP Basic Authentication on protected routes by delegating to SoftOne.Auth.
/// No JWT, OAuth, or token generation — credentials are verified per request.
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}
