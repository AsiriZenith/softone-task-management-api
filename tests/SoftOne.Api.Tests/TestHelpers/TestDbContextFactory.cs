using Microsoft.EntityFrameworkCore;
using SoftOne.Api.Data;

namespace SoftOne.Api.Tests.TestHelpers;

/// <summary>
/// In-memory database setup for tests will be implemented in later phases.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
