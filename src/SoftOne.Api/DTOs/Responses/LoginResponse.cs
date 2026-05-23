namespace SoftOne.Api.DTOs.Responses;

public class LoginResponse
{
    public bool Success { get; init; } = true;

    public string Message { get; init; } = "Credentials valid";
}
