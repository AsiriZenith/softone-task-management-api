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

    public async Task<PagedResponse<TaskResponse>> GetAllAsync(
        bool? isCompleted = null,
        TaskPriority? priority = null,
        string sortBy = "createdAt",
        bool sortDescending = false,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);

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

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<TaskResponse>
        {
            Items = tasks.Select(MapToResponse).ToList(),
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount,
            TotalPages = Pagination.CalculateTotalPages(totalCount, normalizedPageSize)
        };
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
            Status = Status.Todo,
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

    public async Task<TaskResponse?> UpdateTaskStatusAsync(
        int id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task is null)
        {
            return null;
        }

        ApplyStatus(task, request.Status);
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

    private static void ApplyStatus(TaskItem task, Status status)
    {
        task.Status = status;
        task.IsCompleted = status == Status.Completed;
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
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
}
