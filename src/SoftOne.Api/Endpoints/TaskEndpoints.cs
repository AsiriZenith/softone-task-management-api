using FluentValidation;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.DTOs.Responses;
using SoftOne.Api.Services;

namespace SoftOne.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks");

        group.MapGet("/", GetAllTasksAsync)
            .WithName("GetTasks")
            .Produces<ApiSuccessResponse<PagedResponse<TaskResponse>>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:int}", GetTaskByIdAsync)
            .WithName("GetTaskById")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateTaskAsync)
            .WithName("CreateTask")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:int}", UpdateTaskAsync)
            .WithName("UpdateTask")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/status", UpdateTaskStatusAsync)
            .WithName("UpdateTaskStatus")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", DeleteTaskAsync)
            .WithName("DeleteTask")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAllTasksAsync(
        ITaskService taskService,
        bool? isCompleted,
        string? priority,
        string? sortBy,
        string? sortDirection,
        int page = 1,
        int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        if (!TaskQueryParameters.TryParsePriority(priority, out var priorityFilter, out var priorityError))
        {
            return priorityError!;
        }

        var sortDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var tasks = await taskService.GetAllAsync(
            isCompleted,
            priorityFilter,
            sortBy ?? "createdAt",
            sortDescending,
            page,
            pageSize,
            cancellationToken);

        return Results.Ok(new ApiSuccessResponse<PagedResponse<TaskResponse>> { Data = tasks });
    }

    private static async Task<IResult> GetTaskByIdAsync(
        int id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return EndpointResults.TaskNotFound();
        }

        return Results.Ok(new ApiSuccessResponse<TaskResponse> { Data = task });
    }

    private static async Task<IResult> CreateTaskAsync(
        CreateTaskRequest request,
        ITaskService taskService,
        IValidator<CreateTaskRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationError = await EndpointValidation.ValidateAsync(request, validator, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var task = await taskService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/tasks/{task.Id}", new ApiSuccessResponse<TaskResponse> { Data = task });
    }

    private static async Task<IResult> UpdateTaskAsync(
        int id,
        UpdateTaskRequest request,
        ITaskService taskService,
        IValidator<UpdateTaskRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationError = await EndpointValidation.ValidateAsync(request, validator, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var task = await taskService.UpdateAsync(id, request, cancellationToken);
        if (task is null)
        {
            return EndpointResults.TaskNotFound();
        }

        return Results.Ok(new ApiSuccessResponse<TaskResponse> { Data = task });
    }

    private static async Task<IResult> UpdateTaskStatusAsync(
        int id,
        UpdateTaskStatusRequest request,
        ITaskService taskService,
        IValidator<UpdateTaskStatusRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationError = await EndpointValidation.ValidateAsync(request, validator, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var task = await taskService.UpdateTaskStatusAsync(id, request, cancellationToken);
        if (task is null)
        {
            return EndpointResults.TaskNotFound();
        }

        return Results.Ok(new ApiSuccessResponse<TaskResponse> { Data = task });
    }

    private static async Task<IResult> DeleteTaskAsync(
        int id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var deleted = await taskService.SoftDeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return EndpointResults.TaskNotFound();
        }

        return Results.NoContent();
    }
}
