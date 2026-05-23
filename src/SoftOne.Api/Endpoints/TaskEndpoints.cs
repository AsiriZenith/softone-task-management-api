using FluentValidation;
using SoftOne.Api.Data.Enums;
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
            .Produces<ApiSuccessResponse<IReadOnlyList<TaskResponse>>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", GetTaskByIdAsync)
            .WithName("GetTaskById")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateTaskAsync)
            .WithName("CreateTask")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", UpdateTaskAsync)
            .WithName("UpdateTask")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/complete", CompleteTaskAsync)
            .WithName("CompleteTask")
            .Produces<ApiSuccessResponse<TaskResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", DeleteTaskAsync)
            .WithName("DeleteTask")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAllTasksAsync(
        ITaskService taskService,
        bool? isCompleted,
        string? priority,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken)
    {
        if (!TryParsePriority(priority, out var priorityFilter, out var priorityError))
        {
            return priorityError!;
        }

        var sortDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var tasks = await taskService.GetAllAsync(
            isCompleted,
            priorityFilter,
            sortBy ?? "createdAt",
            sortDescending,
            cancellationToken);

        return Results.Ok(new ApiSuccessResponse<IReadOnlyList<TaskResponse>> { Data = tasks });
    }

    private static async Task<IResult> GetTaskByIdAsync(
        int id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFoundResult();
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
            return NotFoundResult();
        }

        return Results.Ok(new ApiSuccessResponse<TaskResponse> { Data = task });
    }

    private static async Task<IResult> CompleteTaskAsync(
        int id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var task = await taskService.MarkCompletedAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFoundResult();
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
            return NotFoundResult();
        }

        return Results.NoContent();
    }

    private static IResult NotFoundResult() =>
        Results.Json(
            ErrorResponse.CreateFailure("Task not found."),
            statusCode: StatusCodes.Status404NotFound);

    private static bool TryParsePriority(string? priority, out TaskPriority? value, out IResult? error)
    {
        value = null;
        error = null;

        if (string.IsNullOrWhiteSpace(priority))
        {
            return true;
        }

        if (Enum.TryParse<TaskPriority>(priority, ignoreCase: true, out var parsed)
            && Enum.IsDefined(typeof(TaskPriority), parsed))
        {
            value = parsed;
            return true;
        }

        error = Results.BadRequest(ErrorResponse.CreateFailure(
            "Validation failed",
            ["Priority must be a valid value (Low, Medium, or High)."]));

        return false;
    }
}
