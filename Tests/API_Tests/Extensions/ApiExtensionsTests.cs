using API.Extensions;
using API.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests.API_Tests.Extensions;

public class ApiExtensionsTests
{
    [Fact]
    public void AddApiConfiguration_ShouldRegisterControllers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiConfiguration();

        // Assert
        // Check for MVC/controller services
        var controllerServices = services.Any(s =>
            s.ServiceType.FullName != null &&
            s.ServiceType.FullName.Contains("Mvc"));
        controllerServices.Should().BeTrue();
    }

    [Fact]
    public void AddApiConfiguration_ShouldConfigureApiBehaviorOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiConfiguration();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<ApiBehaviorOptions>>();

        options.Should().NotBeNull();
        var behaviorOptions = options!.Value;

        behaviorOptions.SuppressModelStateInvalidFilter.Should().BeTrue();
        behaviorOptions.SuppressMapClientErrors.Should().BeTrue();
        behaviorOptions.InvalidModelStateResponseFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddApiConfiguration_ShouldSetCustomModelStateResponseFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiConfiguration();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<ApiBehaviorOptions>>();

        var factory = options!.Value.InvalidModelStateResponseFactory;
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddCorsConfiguration_ShouldRegisterCorsServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCorsConfiguration();

        // Assert
        var corsService = services.Any(s => s.ServiceType.Name.Contains("Cors"));
        corsService.Should().BeTrue();
    }

    [Fact]
    public void UseCorsConfiguration_ShouldConfigureCorsPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCorsConfiguration();
        services.AddRouting();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        Action act = () => app.UseCorsConfiguration();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddApiConfiguration_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiConfiguration();
        services.AddApiConfiguration();

        // Assert
        Action act = () => services.BuildServiceProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddCorsConfiguration_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCorsConfiguration();
        services.AddCorsConfiguration();

        // Assert
        Action act = () => services.BuildServiceProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddHealthCheckServices_ShouldRegisterBaseApiCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddHealthCheckServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        // Assert
        options.Value.Registrations.Should().Contain(r => r.Name == "API");
    }

    [Fact]
    public void AddHealthCheckServices_WhenPostgresConnectionIsPresent_ShouldRegisterPostgreSQLCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?> {
            {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=test"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        services.AddHealthCheckServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        // Assert
        options.Value.Registrations.Should().Contain(r => r.Name == "PostgreSQL");
    }

    [Fact]
    public void AddHealthCheckServices_WhenSeqUrlIsPresent_ShouldRegisterSeqCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?> {
            {"Serilog:WriteTo:1:Args:serverUrl", "http://localhost:5341"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        services.AddHealthCheckServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        // Assert
        options.Value.Registrations.Should().Contain(r => r.Name == "Seq");
    }

    [Fact]
    public void AddHealthCheckServices_WhenBothConfigsAreMissing_ShouldOnlyRegisterApiCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddHealthCheckServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        // Assert
        options.Value.Registrations.Should().HaveCount(1);
        options.Value.Registrations.Should().Contain(r => r.Name == "API");
    }
}
