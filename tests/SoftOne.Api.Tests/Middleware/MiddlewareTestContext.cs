using System.Text;
using Microsoft.AspNetCore.Http;

namespace SoftOne.Api.Tests.Middleware;

internal static class MiddlewareTestContext
{
    public static HttpContext Create(
        string path = "/api/tasks",
        string method = "GET",
        string? authorizationHeader = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;
        context.Response.Body = new MemoryStream();

        if (authorizationHeader is not null)
        {
            context.Request.Headers.Authorization = authorizationHeader;
        }

        return context;
    }

    public static string CreateBasicAuthorizationHeader(string username, string password)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return $"Basic {credentials}";
    }

    public static async Task<string> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
