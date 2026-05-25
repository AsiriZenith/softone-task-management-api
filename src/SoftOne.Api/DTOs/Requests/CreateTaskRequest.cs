using SoftOne.Api.Data.Enums;

namespace SoftOne.Api.DTOs.Requests;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority? Priority { get; set; }

    public DateOnly? DueDate { get; set; }
}
