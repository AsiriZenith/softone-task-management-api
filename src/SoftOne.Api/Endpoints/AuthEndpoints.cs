using FluentValidation;
using SoftOne.Api.DTOs.Responses;
using SoftOne.Auth.Interfaces;
using SoftOne.Auth.Models;

namespace SoftOne.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthService authService,
        IValidator<LoginRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationError = await EndpointValidation.ValidateAsync(request, validator, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var result = authService.ValidateCredentials(request.Username, request.Password);

        if (!result.IsAuthenticated)
        {
            return Results.Json(
                ErrorResponse.CreateFailure("Invalid username or password."),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Results.Ok(new LoginResponse());
    }
}
