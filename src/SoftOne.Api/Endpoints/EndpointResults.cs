using SoftOne.Api.DTOs.Responses;

namespace SoftOne.Api.Endpoints;

public static class EndpointResults
{
    public static IResult NotFound(string message) =>
        Results.Json(
            ErrorResponse.CreateFailure(message),
            statusCode: StatusCodes.Status404NotFound);

    public static IResult TaskNotFound() => NotFound("Task not found.");
}
