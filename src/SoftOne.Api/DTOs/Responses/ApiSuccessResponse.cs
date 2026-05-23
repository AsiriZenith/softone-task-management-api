namespace SoftOne.Api.DTOs.Responses;

public class ApiSuccessResponse<T>
{
    public bool Success { get; init; } = true;

    public T Data { get; init; } = default!;
}
