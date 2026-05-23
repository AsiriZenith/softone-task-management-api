namespace SoftOne.Auth.Models;

public sealed record AuthResult
{
    public bool IsAuthenticated { get; init; }

    public string? Username { get; init; }

    public string? ErrorMessage { get; init; }

    public static AuthResult Authenticated(string username) =>
        new()
        {
            IsAuthenticated = true,
            Username = username
        };

    public static AuthResult Failed(string errorMessage) =>
        new()
        {
            IsAuthenticated = false,
            ErrorMessage = errorMessage
        };
}
