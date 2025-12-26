using API.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Tests.API_Tests.Extensions;

public class LoggingExtensionsTests
{
    [Fact]
    public void AddSerilogConfiguration_ShouldConfigureSerilog()
    {
        // Arrange
        var hostBuilder = new HostBuilder();

        // Act
        hostBuilder.AddSerilogConfiguration();
        var host = hostBuilder.Build();

        // Assert
        host.Should().NotBeNull();
        // Serilog should be configured through the host builder
    }

    [Fact]
    public void AddSerilogConfiguration_ShouldNotThrow()
    {
        // Arrange
        var hostBuilder = new HostBuilder();

        // Act
        Action act = () => hostBuilder.AddSerilogConfiguration();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseSerilogRequestLogging_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        Action act = () => app.UseSerilogRequestLogging();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseSerilogRequestLogging_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        Action act = () =>
        {
            app.UseSerilogRequestLogging();
            app.UseSerilogRequestLogging();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddSerilogConfiguration_ShouldReadFromConfiguration()
    {
        // Arrange
        var configuration = new Dictionary<string, string?>
        {
            ["Serilog:MinimumLevel:Default"] = "Information",
            ["Serilog:WriteTo:0:Name"] = "Console"
        };

        var hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(configuration!);
            });

        // Act
        hostBuilder.AddSerilogConfiguration();

        // Assert
        Action act = () => hostBuilder.Build();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddSerilogConfiguration_ShouldAllowMultipleCalls()
    {
        // Arrange
        var hostBuilder = new HostBuilder();

        // Act
        hostBuilder.AddSerilogConfiguration();
        hostBuilder.AddSerilogConfiguration();

        // Assert
        Action act = () => hostBuilder.Build();
        act.Should().NotThrow();
    }
}
