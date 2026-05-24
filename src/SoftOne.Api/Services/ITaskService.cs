using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.DTOs.Responses;

namespace SoftOne.Api.Services;

public interface ITaskService
{
    Task<PagedResponse<TaskResponse>> GetAllAsync(
        bool? isCompleted = null,
        TaskPriority? priority = null,
        string sortBy = "createdAt",
        bool sortDescending = false,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default);

    Task<TaskResponse?> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default);

    Task<TaskResponse?> UpdateTaskStatusAsync(
        int id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
