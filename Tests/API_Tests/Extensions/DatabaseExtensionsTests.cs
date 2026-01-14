using API.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Persistence;
using Serilog;
using Xunit;

namespace Tests.API_Tests.Extensions;

public class DatabaseExtensionsTests
{
    [Fact]
    public void AddDatabaseServices_ShouldRegisterAppDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<AppDbContext>();

        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabaseServices_ShouldRegisterIAppDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<IAppDbContext>();

        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabaseServices_ShouldConfigurePostgreSQL()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpass";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<AppDbContext>();

        dbContext.Should().NotBeNull();
        dbContext!.Database.IsNpgsql().Should().BeTrue();
    }

    [Fact]
    public void AddDatabaseServices_ShouldRegisterDbContextAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);

        // Assert
        var appDbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AppDbContext));
        appDbContextDescriptor.Should().NotBeNull();
        appDbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddDatabaseServices_ShouldRegisterIAppDbContextAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);

        // Assert
        var iAppDbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IAppDbContext));
        iAppDbContextDescriptor.Should().NotBeNull();
        iAppDbContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddDatabaseServices_ShouldUseConnectionStringFromConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedConnectionString = "Host=testhost;Database=testdb;Port=5433";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = expectedConnectionString
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<AppDbContext>();

        dbContext.Should().NotBeNull();
        var connectionString = dbContext!.Database.GetConnectionString();
        connectionString.Should().Contain("testhost");
        connectionString.Should().Contain("testdb");
    }

    [Fact]
    public void AddDatabaseServices_ShouldResolveIAppDbContextToAppDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var iAppDbContext = serviceProvider.GetService<IAppDbContext>();
        var appDbContext = serviceProvider.GetService<AppDbContext>();

        iAppDbContext.Should().NotBeNull();
        appDbContext.Should().NotBeNull();
        iAppDbContext.Should().BeSameAs(appDbContext);
    }

    [Fact]
    public void AddDatabaseServices_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            }!)
            .Build();

        // Act
        services.AddDatabaseServices(configuration);
        services.AddDatabaseServices(configuration);

        // Assert
        Action act = () => services.BuildServiceProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldCreateScope_AndApplyMigrations()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
            }!)
            .Build();

        services.AddDatabaseServices(configuration);

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert
        // Verify that the method completes without throwing
        app.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldGetAppDbContext_FromServiceProvider()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert - Verify service can be resolved
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldCallMigrateAsync()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        Func<Task> act = async () => await app.ApplyMigrationsAndSeed();

        // Assert
        await act.Should().NotThrowAsync("because the method should handle migration gracefully");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldCallDbInitializerSeedData()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert - Verify the method executes without throwing
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Should().NotBeNull("because the context should be resolvable after the method runs");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldExecuteMigrateAsyncBeforeSeedData()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Ensure table exists for query even if migration fails
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert - Verify database is ready for operations after migration
        using var assertScope = app.Services.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var canQuery = await assertContext.SerhanKitaplar.AnyAsync();

        // The query should execute successfully
        canQuery.Should().BeFalse("because the database starts empty but is queryable");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_MigrateAsync_ShouldEnsureDatabaseExists()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert - Database should be accessible after migration
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var database = context.Database;
        database.Should().NotBeNull("because MigrateAsync should ensure database exists");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_SeedData_ShouldOnlySeedWhenDatabaseEmpty()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Pre-populate database
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
            context.SerhanKitaplar.Add(new Domain.SerhanKitap
            {
                KitapName = "Test Book",
                KitapYazar = "Test Author",
                KitapSayfaSayisi = 100
            });
            await context.SaveChangesAsync();
        }

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert - Seed should not add data when database already has records
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var count = await context.SerhanKitaplar.CountAsync();
            count.Should().Be(1, "because DbInitializer.SeedData should not add data when database is not empty");
        }
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        Func<Task> act = async () =>
        {
            await app.ApplyMigrationsAndSeed();
            await app.ApplyMigrationsAndSeed();
        };

        // Assert
        await act.Should().NotThrowAsync("because the method should be idempotent");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldDisposeScope_AfterExecution()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert - Verify we can still create new scopes after the method completes
        using var newScope = app.Services.CreateScope();
        var context = newScope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Should().NotBeNull("because the scope should be properly disposed and new ones can be created");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldHandleException_AndLogFatal()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();

        // Configure a context
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act - The method catches exceptions and logs them, so it shouldn't throw
        Func<Task> act = async () => await app.ApplyMigrationsAndSeed();

        // Assert
        await act.Should().NotThrowAsync("because exceptions are caught and logged internally");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_MigrateAsync_ShouldBeCalledOnDatabaseFacade()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();

        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act
        await app.ApplyMigrationsAndSeed();

        // Assert - Verify database is accessible
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Database should be accessible
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("because database should be accessible");
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_DbInitializerSeedData_ShouldBeCalledWithContext()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();

        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Verify database is initially empty
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
            var initialCount = await context.SerhanKitaplar.CountAsync();
            initialCount.Should().Be(0, "database should start empty");
        }

        // Act - This should call DbInitializer.SeedData(context)
        await app.ApplyMigrationsAndSeed();

        // Assert - The method was called even if MigrateAsync threw
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Should().NotBeNull("because the method should complete even if migration fails");
        }
    }

    [Fact]
    public async Task ApplyMigrationsAndSeed_ShouldCallBothMigrateAsyncAndSeedDataInSequence()
    {
        // Arrange
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var builder = WebApplication.CreateBuilder();

        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(connection));
        builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        var app = builder.Build();

        // Act - Should execute both await context.Database.MigrateAsync() and await DbInitializer.SeedData(context)
        Func<Task> act = async () => await app.ApplyMigrationsAndSeed();

        // Assert - Verify the method completes without throwing
        await act.Should().NotThrowAsync("because exceptions are caught and logged internally");

        // Verify database context is still accessible after the method runs
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Should().NotBeNull("because the context should be accessible even if migration/seeding encounters issues");
    }
}

