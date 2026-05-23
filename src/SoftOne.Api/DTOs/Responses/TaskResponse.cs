using SoftOne.Api.Data.Enums;

namespace SoftOne.Api.DTOs.Responses;

public class TaskResponse
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public TaskPriority Priority { get; init; }

    public DateTime? DueDate { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
