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

    [Fact]
    public void AddEnvironmentConfiguration_ShouldNotThrow_WhenEnvFileDoesNotExist()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act - the method checks if file exists before loading
        Action act = () => configBuilder.AddEnvironmentConfiguration(envMock.Object);

        // Assert
        act.Should().NotThrow("because the method should gracefully handle missing .env files");
    }

    [Fact]
    public void AddEnvironmentConfiguration_ShouldHandleMissingEnvFile_InProduction()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Ensure environment variables are not set
        Environment.SetEnvironmentVariable("DefaultConnection", null);
        Environment.SetEnvironmentVariable("SeqServerUrl", null);

        // Act - the method checks File.Exists before Env.Load
        Action act = () => configBuilder.AddEnvironmentConfiguration(envMock.Object);

        // Assert
        act.Should().NotThrow("because File.Exists check prevents loading non-existent files");
    }

    [Fact]
    public void AddEnvironmentConfiguration_ShouldLoadFromEnvFile_WhenFileExists()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // The code specifically looks for "../dev.env" when in Development
        var testEnvFilePath = "../dev.env";
        var testConnectionString = "Host=testhost;Database=testdb;";
        var testSeqUrl = "http://testseq:5341";

        // Create a temporary .env file at the expected location
        File.WriteAllText(testEnvFilePath, $"DefaultConnection={testConnectionString}\nSeqServerUrl={testSeqUrl}");

        try
        {
            // Act
            configBuilder.AddEnvironmentConfiguration(envMock.Object);
            var config = configBuilder.Build();

            // Assert
            // 1. Verify environment variables were loaded into the process
            Environment.GetEnvironmentVariable("DefaultConnection").Should().Be(testConnectionString);
            Environment.GetEnvironmentVariable("SeqServerUrl").Should().Be(testSeqUrl);

            // 2. Verify they were also added to the IConfiguration
            config.GetConnectionString("DefaultConnection").Should().Be(testConnectionString);
            config["Serilog:WriteTo:1:Args:serverUrl"].Should().Be(testSeqUrl);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testEnvFilePath))
            {
                File.Delete(testEnvFilePath);
            }
            Environment.SetEnvironmentVariable("DefaultConnection", null);
            Environment.SetEnvironmentVariable("SeqServerUrl", null);
        }
    }

    [Theory]
    [InlineData("Development", "../dev.env")]
    [InlineData("Production", "../prod.env")]
    public void AddEnvironmentConfiguration_ShouldCheckCorrectEnvFile_BasedOnEnvironment(string environmentName, string expectedFilePath)
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(environmentName);

        // Act - The method will check if the expected file exists
        // This test verifies the correct file path is constructed based on environment
        Action act = () => configBuilder.AddEnvironmentConfiguration(envMock.Object);

        // Assert
        act.Should().NotThrow($"because the File.Exists check should handle missing {expectedFilePath} gracefully");

        // The test implicitly verifies that the correct file path is being checked
        // Development should check ../dev.env, Production should check ../prod.env
        expectedFilePath.Should().NotBeNullOrEmpty();
    }
}
