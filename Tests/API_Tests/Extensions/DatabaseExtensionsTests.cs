using API.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
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
}
