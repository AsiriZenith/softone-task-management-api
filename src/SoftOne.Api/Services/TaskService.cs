using Microsoft.EntityFrameworkCore;
using SoftOne.Api.Data;
using SoftOne.Api.Data.Entities;
using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.DTOs.Responses;

namespace SoftOne.Api.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(
        bool? isCompleted = null,
        TaskPriority? priority = null,
        string sortBy = "createdAt",
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tasks.AsNoTracking();

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var tasks = await query.ToListAsync(cancellationToken);
        return tasks.Select(MapToResponse).ToList();
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return task is null ? null : MapToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority ?? TaskPriority.Medium,
            DueDate = request.DueDate,
            IsCompleted = false,
            IsDeleted = false
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task is null)
        {
            return null;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority ?? task.Priority;
        task.DueDate = request.DueDate;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(task);
    }

    public async Task<TaskResponse?> MarkCompletedAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task is null)
        {
            return null;
        }

        task.IsCompleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(task);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task is null)
        {
            return false;
        }

        task.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static IQueryable<TaskItem> ApplySorting(
        IQueryable<TaskItem> query,
        string sortBy,
        bool sortDescending)
    {
        var field = sortBy.Trim().ToLowerInvariant();

        return field switch
        {
            "duedate" => sortDescending
                ? query.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.DueDate).ThenBy(t => t.CreatedAt),
            _ => sortDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt)
        };
    }

    private static TaskResponse MapToResponse(TaskItem task) =>
        new()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
}
