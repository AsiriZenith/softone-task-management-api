namespace SoftOne.Api.DTOs.Responses;

public sealed record ErrorResponse
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ErrorResponse CreateFailure(string message, IReadOnlyList<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors ?? Array.Empty<string>()
        };
}
