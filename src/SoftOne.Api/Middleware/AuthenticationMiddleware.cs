using System.Text;
using SoftOne.Auth.Interfaces;

namespace SoftOne.Api.Middleware;

public class AuthenticationMiddleware
{
    private const string BasicScheme = "Basic";
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        if (!AuthenticationMiddlewareExtensions.RequiresAuthentication(context))
        {
            await _next(context);
            return;
        }

        if (!TryGetBasicCredentials(context, out var username, out var password))
        {
            await MiddlewareResponseWriter.WriteErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized");
            return;
        }

        var result = authService.ValidateCredentials(username, password);

        if (!result.IsAuthenticated)
        {
            await MiddlewareResponseWriter.WriteErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized");
            return;
        }

        context.Items["AuthenticatedUsername"] = result.Username;

        await _next(context);
    }

    private static bool TryGetBasicCredentials(
        HttpContext context,
        out string username,
        out string password)
    {
        username = string.Empty;
        password = string.Empty;

        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return false;
        }

        var headerValue = authorizationHeader.ToString();
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return false;
        }

        var parts = headerValue.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !parts[0].Equals(BasicScheme, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        string decodedCredentials;
        try
        {
            var credentialBytes = Convert.FromBase64String(parts[1].Trim());
            decodedCredentials = Encoding.UTF8.GetString(credentialBytes);
        }
        catch (FormatException)
        {
            return false;
        }

        var separatorIndex = decodedCredentials.IndexOf(':');
        if (separatorIndex <= 0 || separatorIndex == decodedCredentials.Length - 1)
        {
            return false;
        }

        username = decodedCredentials[..separatorIndex];
        password = decodedCredentials[(separatorIndex + 1)..];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return true;
    }
}
