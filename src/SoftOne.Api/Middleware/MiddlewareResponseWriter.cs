using System.Text.Json;
using SoftOne.Api.DTOs.Responses;

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

        var response = ErrorResponse.CreateFailure(message, errors);

        await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions);
    }
}
