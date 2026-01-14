using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        // Ensure the database is created
        context.Database.EnsureCreated();

        return context;
    }

    public static AppDbContext CreateInMemoryDbContext(string databaseName)
    {
        // For Sqlite in-memory, a new connection means a new database, 
        // effectively providing the same isolation as a unique database name.
        return CreateInMemoryDbContext();
    }
}
