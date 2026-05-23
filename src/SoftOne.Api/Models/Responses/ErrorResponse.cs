namespace SoftOne.Api.Models.Responses;

public class ErrorResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
}
