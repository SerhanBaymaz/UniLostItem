using API.Extensions;
using API.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<ApiBehaviorOptions>>();

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
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<ApiBehaviorOptions>>();

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
}
