using FluentValidation;
using FluentValidation.Results;
using SoftOne.Api.DTOs.Responses;

namespace SoftOne.Api.Endpoints;

internal static class EndpointValidation
{
    public static IResult? ToValidationResult(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return null;
        }

        return Results.BadRequest(ErrorResponse.CreateFailure(
            "Validation failed",
            validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
    }

    public static async Task<IResult?> ValidateAsync<T>(
        T request,
        IValidator<T> validator,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        return ToValidationResult(validationResult);
    }
}
