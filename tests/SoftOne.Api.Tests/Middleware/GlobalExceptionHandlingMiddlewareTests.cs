using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SoftOne.Api.Middleware;
using Xunit;

namespace SoftOne.Api.Tests.Middleware;

public class GlobalExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNextThrows_ReturnsSanitizedInternalServerError()
    {
        var context = MiddlewareTestContext.Create();
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        RequestDelegate next = _ => throw new InvalidOperationException("Sensitive database connection failed.");

        var middleware = new GlobalExceptionHandlingMiddleware(next, logger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var body = await MiddlewareTestContext.ReadResponseBodyAsync(context);
        body.Should().Contain("\"success\":false");
        body.Should().Contain("\"message\":\"An unexpected error occurred.\"");
        body.Should().Contain("\"errors\":[]");
        body.Should().NotContain("Sensitive");
        body.Should().NotContain("stack");
        body.Should().NotContain("InvalidOperationException");
    }

    [Fact]
    public async Task InvokeAsync_WhenNextSucceeds_DoesNotModifyResponse()
    {
        var context = MiddlewareTestContext.Create();
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlingMiddleware(next, logger.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
