using SoftOne.Api.Data.Entities;

namespace SoftOne.Api.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
