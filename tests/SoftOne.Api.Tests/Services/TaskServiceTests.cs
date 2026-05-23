using FluentAssertions;
using SoftOne.Api.Data;
using SoftOne.Api.Data.Entities;
using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.Services;
using SoftOne.Api.Tests.TestHelpers;

namespace SoftOne.Api.Tests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _sut = new TaskService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsTask()
    {
        var request = new CreateTaskRequest
        {
            Title = "New task",
            Description = "Details",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.Date.AddDays(3)
        };

        var result = await _sut.CreateAsync(request);

        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Priority.Should().Be(TaskPriority.High);
        result.IsCompleted.Should().BeFalse();
        result.DueDate.Should().Be(request.DueDate);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesFields()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest
        {
            Title = "Original",
            Priority = TaskPriority.Low
        });

        var update = new UpdateTaskRequest
        {
            Title = "Updated",
            Description = "Changed",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.Date.AddDays(10)
        };

        var result = await _sut.UpdateAsync(created.Id, update);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        result.Description.Should().Be("Changed");
        result.Priority.Should().Be(TaskPriority.High);
        result.DueDate.Should().Be(update.DueDate);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkCompletedAsync_ExistingTask_SetsIsCompleted()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest { Title = "To complete" });

        var result = await _sut.MarkCompletedAsync(created.Id);

        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingTask_HidesFromQueries()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest { Title = "To delete" });

        var deleted = await _sut.SoftDeleteAsync(created.Id);

        deleted.Should().BeTrue();

        var byId = await _sut.GetByIdAsync(created.Id);
        var all = await _sut.GetAllAsync();

        byId.Should().BeNull();
        all.Should().NotContain(t => t.Id == created.Id);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedTasks()
    {
        await _sut.CreateAsync(new CreateTaskRequest { Title = "Active task" });

        _context.Tasks.Add(new TaskItem
        {
            Title = "Deleted task",
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow,
            Priority = TaskPriority.Medium
        });
        await _context.SaveChangesAsync();

        var results = await _sut.GetAllAsync();

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Active task");
    }

    [Fact]
    public async Task GetAllAsync_FilterByCompletionStatus_ReturnsMatchingTasks()
    {
        var open = await _sut.CreateAsync(new CreateTaskRequest { Title = "Open" });
        var done = await _sut.CreateAsync(new CreateTaskRequest { Title = "Done" });
        await _sut.MarkCompletedAsync(done.Id);

        var completedOnly = await _sut.GetAllAsync(isCompleted: true);
        var openOnly = await _sut.GetAllAsync(isCompleted: false);

        completedOnly.Should().ContainSingle(t => t.Id == done.Id);
        openOnly.Should().ContainSingle(t => t.Id == open.Id);
    }

    [Fact]
    public async Task GetAllAsync_FilterByPriority_ReturnsMatchingTasks()
    {
        await _sut.CreateAsync(new CreateTaskRequest { Title = "Low", Priority = TaskPriority.Low });
        var high = await _sut.CreateAsync(new CreateTaskRequest { Title = "High", Priority = TaskPriority.High });

        var results = await _sut.GetAllAsync(priority: TaskPriority.High);

        results.Should().ContainSingle(t => t.Id == high.Id);
        results[0].Title.Should().Be("High");
    }

    [Fact]
    public async Task GetAllAsync_SortByCreatedAtDescending_ReturnsNewestFirst()
    {
        var older = await _sut.CreateAsync(new CreateTaskRequest { Title = "Older" });
        await Task.Delay(10);
        var newer = await _sut.CreateAsync(new CreateTaskRequest { Title = "Newer" });

        var results = await _sut.GetAllAsync(sortBy: "createdAt", sortDescending: true);

        results.Select(t => t.Id).Should().ContainInOrder(newer.Id, older.Id);
    }

    [Fact]
    public async Task GetAllAsync_SortByDueDateAscending_OrdersByDueDate()
    {
        await _sut.CreateAsync(new CreateTaskRequest
        {
            Title = "Later",
            DueDate = DateTime.UtcNow.Date.AddDays(10)
        });
        var sooner = await _sut.CreateAsync(new CreateTaskRequest
        {
            Title = "Sooner",
            DueDate = DateTime.UtcNow.Date.AddDays(1)
        });

        var results = await _sut.GetAllAsync(sortBy: "dueDate", sortDescending: false);

        results[0].Id.Should().Be(sooner.Id);
        results[^1].Title.Should().Be("Later");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.UpdateAsync(9999, new UpdateTaskRequest { Title = "Missing" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task MarkCompletedAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.MarkCompletedAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteAsync_NotFound_ReturnsFalse()
    {
        var result = await _sut.SoftDeleteAsync(9999);

        result.Should().BeFalse();
    }
}
