using System.Text.Json;
using SoftOne.Api.Models.Responses;

namespace SoftOne.Api.Middleware;

internal static class MiddlewareResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message,
        IReadOnlyList<string>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? Array.Empty<string>()
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions);
    }
}
