using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SoftOne.Auth.Interfaces;
using SoftOne.Auth.Models;
using SoftOne.Api.Middleware;
using Xunit;

namespace SoftOne.Api.Tests.Middleware;

public class AuthenticationMiddlewareTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    [Fact]
    public async Task InvokeAsync_MissingAuthorizationHeader_ReturnsUnauthorized()
    {
        var context = MiddlewareTestContext.Create(authorizationHeader: null);
        var middleware = CreateMiddleware(_ => SetSuccessResponse(context));

        await middleware.InvokeAsync(context, _authServiceMock.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        var body = await MiddlewareTestContext.ReadResponseBodyAsync(context);
        body.Should().Contain("\"success\":false");
        body.Should().Contain("\"message\":\"Unauthorized\"");
        body.Should().Contain("\"errors\":[]");
    }

    [Fact]
    public async Task InvokeAsync_InvalidAuthorizationFormat_ReturnsUnauthorized()
    {
        var context = MiddlewareTestContext.Create(authorizationHeader: "Bearer some-token");
        var middleware = CreateMiddleware(_ => SetSuccessResponse(context));

        await middleware.InvokeAsync(context, _authServiceMock.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_InvalidCredentials_ReturnsUnauthorized()
    {
        _authServiceMock
            .Setup(s => s.ValidateCredentials("admin", "wrong"))
            .Returns(AuthResult.Failed("Invalid username or password."));

        var header = MiddlewareTestContext.CreateBasicAuthorizationHeader("admin", "wrong");
        var context = MiddlewareTestContext.Create(authorizationHeader: header);
        var middleware = CreateMiddleware(_ => SetSuccessResponse(context));

        await middleware.InvokeAsync(context, _authServiceMock.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _authServiceMock.Verify(s => s.ValidateCredentials("admin", "wrong"), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_OptionsRequest_SkipsAuthentication()
    {
        var context = MiddlewareTestContext.Create(method: "OPTIONS", authorizationHeader: null);
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return SetSuccessResponse(context);
        });

        await middleware.InvokeAsync(context, _authServiceMock.Object);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        _authServiceMock.Verify(
            s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ValidCredentials_CallsNextMiddleware()
    {
        _authServiceMock
            .Setup(s => s.ValidateCredentials("admin", "Admin@123"))
            .Returns(AuthResult.Authenticated("admin"));

        var header = MiddlewareTestContext.CreateBasicAuthorizationHeader("admin", "Admin@123");
        var context = MiddlewareTestContext.Create(authorizationHeader: header);
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return SetSuccessResponse(context);
        });

        await middleware.InvokeAsync(context, _authServiceMock.Object);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.Items["AuthenticatedUsername"].Should().Be("admin");
    }

    private static AuthenticationMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next);

    private static Task SetSuccessResponse(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        return Task.CompletedTask;
    }
}
