using SoftOne.Api.Data;
using SoftOne.Api.Data.Entities;

namespace SoftOne.Api.Services;

/// <summary>
/// Task business logic will be implemented in Phase 7.
/// </summary>
public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
