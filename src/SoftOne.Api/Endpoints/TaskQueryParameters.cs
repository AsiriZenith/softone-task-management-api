using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Responses;

namespace SoftOne.Api.Endpoints;

public static class TaskQueryParameters
{
    public static bool TryParsePriority(string? priority, out TaskPriority? value, out IResult? error)
    {
        value = null;
        error = null;

        if (string.IsNullOrWhiteSpace(priority))
        {
            return true;
        }

        if (Enum.TryParse<TaskPriority>(priority, ignoreCase: true, out var parsed)
            && Enum.IsDefined(typeof(TaskPriority), parsed))
        {
            value = parsed;
            return true;
        }

        error = Results.BadRequest(ErrorResponse.CreateFailure(
            "Validation failed",
            ["Priority must be a valid value (Low, Medium, or High)."]));

        return false;
    }
}
