using API.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Tests.API_Tests.Extensions;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void AddEnvironmentConfiguration_ShouldNotThrow_InDevelopmentEnvironment()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        Action act = () => configBuilder.AddEnvironmentConfiguration(envMock.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddEnvironmentConfiguration_ShouldNotThrow_InProductionEnvironment()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        Action act = () => configBuilder.AddEnvironmentConfiguration(envMock.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddEnvironmentConfiguration_ShouldAddConnectionString_WhenEnvironmentVariableExists()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var testConnectionString = "Host=localhost;Database=test;";
        var testSeqUrl = "http://localhost:5341";

        // Set environment variables
        Environment.SetEnvironmentVariable("DefaultConnection", testConnectionString);
        Environment.SetEnvironmentVariable("SeqServerUrl", testSeqUrl);

        try
        {
            // Act
            configBuilder.AddEnvironmentConfiguration(envMock.Object);
            var config = configBuilder.Build();

            // Assert
            var connectionString = config.GetConnectionString("DefaultConnection");
            var seqUrl = config["Serilog:WriteTo:1:Args:serverUrl"];

            connectionString.Should().Be(testConnectionString);
            seqUrl.Should().Be(testSeqUrl);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DefaultConnection", null);
            Environment.SetEnvironmentVariable("SeqServerUrl", null);
        }
    }

    [Fact]
    public void AddEnvironmentConfiguration_ShouldNotAddConfiguration_WhenEnvironmentVariablesAreMissing()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Ensure variables are not set
        Environment.SetEnvironmentVariable("DefaultConnection", null);
        Environment.SetEnvironmentVariable("SeqServerUrl", null);

        // Act
        configBuilder.AddEnvironmentConfiguration(envMock.Object);
        var config = configBuilder.Build();

        // Assert
        var connectionString = config.GetConnectionString("DefaultConnection");
        connectionString.Should().BeNullOrEmpty();
    }

    [Fact]
    public void AddEnvironmentConfiguration_ShouldNotAddConfiguration_WhenOnlyOneEnvironmentVariableExists()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        Environment.SetEnvironmentVariable("DefaultConnection", "test-connection");
        Environment.SetEnvironmentVariable("SeqServerUrl", null);

        try
        {
            // Act
            configBuilder.AddEnvironmentConfiguration(envMock.Object);
            var config = configBuilder.Build();

            // Assert
            // Should not add to config if both variables aren't present
            var connectionString = config.GetConnectionString("DefaultConnection");
            connectionString.Should().BeNullOrEmpty();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("DefaultConnection", null);
        }
    }
}
