namespace SoftOne.Api.Models.Requests;

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
}
