using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Ensure the database is created
        context.Database.EnsureCreated();

        return context;
    }

    public static AppDbContext CreateInMemoryDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        var context = new AppDbContext(options);

        // Ensure the database is created
        context.Database.EnsureCreated();

        return context;
    }
}
