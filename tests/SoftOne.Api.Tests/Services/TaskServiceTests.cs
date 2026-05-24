using FluentAssertions;
using SoftOne.Api.Data;
using SoftOne.Api.Data.Entities;
using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.DTOs.Responses;
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
        result.Status.Should().Be(Status.Todo);
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
    public async Task UpdateTaskStatusAsync_SetCompleted_UpdatesStatusAndIsCompleted()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest { Title = "To complete" });

        var result = await _sut.UpdateTaskStatusAsync(
            created.Id,
            new UpdateTaskStatusRequest { Status = Status.Completed });

        result.Should().NotBeNull();
        result!.Status.Should().Be(Status.Completed);

        var entity = await _context.Tasks.FindAsync(created.Id);
        entity!.IsCompleted.Should().BeTrue();
        entity.Status.Should().Be(Status.Completed);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_SetInProgress_UpdatesStatusAndClearsIsCompleted()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest { Title = "Task" });
        await _sut.UpdateTaskStatusAsync(
            created.Id,
            new UpdateTaskStatusRequest { Status = Status.Completed });

        var result = await _sut.UpdateTaskStatusAsync(
            created.Id,
            new UpdateTaskStatusRequest { Status = Status.InProgress });

        result!.Status.Should().Be(Status.InProgress);
        var entity = await _context.Tasks.FindAsync(created.Id);
        entity!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingTask_HidesFromQueries()
    {
        var created = await _sut.CreateAsync(new CreateTaskRequest { Title = "To delete" });

        var deleted = await _sut.SoftDeleteAsync(created.Id);

        deleted.Should().BeTrue();

        var byId = await _sut.GetByIdAsync(created.Id);
        var all = await _sut.GetAllAsync(pageSize: 50);

        byId.Should().BeNull();
        all.Items.Should().NotContain(t => t.Id == created.Id);
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

        var results = await _sut.GetAllAsync(pageSize: 50);

        results.Items.Should().HaveCount(1);
        results.Items.First().Title.Should().Be("Active task");
    }

    [Fact]
    public async Task GetAllAsync_FilterByCompletionStatus_ReturnsMatchingTasks()
    {
        var open = await _sut.CreateAsync(new CreateTaskRequest { Title = "Open" });
        var done = await _sut.CreateAsync(new CreateTaskRequest { Title = "Done" });
        await _sut.UpdateTaskStatusAsync(
            done.Id,
            new UpdateTaskStatusRequest { Status = Status.Completed });

        var completedOnly = await _sut.GetAllAsync(isCompleted: true, pageSize: 50);
        var openOnly = await _sut.GetAllAsync(isCompleted: false, pageSize: 50);

        completedOnly.Items.Should().ContainSingle(t => t.Id == done.Id);
        openOnly.Items.Should().ContainSingle(t => t.Id == open.Id);
    }

    [Fact]
    public async Task GetAllAsync_FilterByPriority_ReturnsMatchingTasks()
    {
        await _sut.CreateAsync(new CreateTaskRequest { Title = "Low", Priority = TaskPriority.Low });
        var high = await _sut.CreateAsync(new CreateTaskRequest { Title = "High", Priority = TaskPriority.High });

        var results = await _sut.GetAllAsync(priority: TaskPriority.High, pageSize: 50);

        results.Items.Should().ContainSingle(t => t.Id == high.Id);
        results.Items.First().Title.Should().Be("High");
    }

    [Fact]
    public async Task GetAllAsync_SortByCreatedAtDescending_ReturnsNewestFirst()
    {
        var older = await _sut.CreateAsync(new CreateTaskRequest { Title = "Older" });
        await Task.Delay(10);
        var newer = await _sut.CreateAsync(new CreateTaskRequest { Title = "Newer" });

        var results = await _sut.GetAllAsync(sortBy: "createdAt", sortDescending: true, pageSize: 50);

        results.Items.Select(t => t.Id).Should().ContainInOrder(newer.Id, older.Id);
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

        var results = await _sut.GetAllAsync(sortBy: "dueDate", sortDescending: false, pageSize: 50);

        results.Items.First().Id.Should().Be(sooner.Id);
        results.Items.Last().Title.Should().Be("Later");
    }

    [Fact]
    public async Task GetAllAsync_DefaultPagination_ReturnsFirstPageWithMetadata()
    {
        for (var i = 1; i <= 7; i++)
        {
            await _sut.CreateAsync(new CreateTaskRequest { Title = $"Task {i}" });
        }

        var result = await _sut.GetAllAsync();

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(2);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ReturnsRemainingItems()
    {
        for (var i = 1; i <= 7; i++)
        {
            await _sut.CreateAsync(new CreateTaskRequest { Title = $"Task {i}" });
        }

        var result = await _sut.GetAllAsync(page: 2, pageSize: 5);

        result.Page.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(7);
    }

    [Fact]
    public async Task GetAllAsync_PageSizeExceedsMax_CapsAtFifty()
    {
        var result = await _sut.GetAllAsync(pageSize: 100);

        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetAllAsync_WithFilter_TotalCountReflectsFilteredSet()
    {
        await _sut.CreateAsync(new CreateTaskRequest { Title = "Open", Priority = TaskPriority.Low });
        var done = await _sut.CreateAsync(new CreateTaskRequest { Title = "Done", Priority = TaskPriority.High });
        await _sut.UpdateTaskStatusAsync(
            done.Id,
            new UpdateTaskStatusRequest { Status = Status.Completed });

        var result = await _sut.GetAllAsync(isCompleted: true, pageSize: 50);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(t => t.Id == done.Id);
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
    public async Task UpdateTaskStatusAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.UpdateTaskStatusAsync(
            9999,
            new UpdateTaskStatusRequest { Status = Status.Todo });

        result.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteAsync_NotFound_ReturnsFalse()
    {
        var result = await _sut.SoftDeleteAsync(9999);

        result.Should().BeFalse();
    }
}
