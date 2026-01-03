using API.Helpers;
using FluentAssertions;
using Xunit;

namespace Tests.API_Tests.Helpers;

public sealed class EnvLoaderTests : IDisposable
{
    private readonly string _testKey;
    private readonly string _testValue;
    private readonly List<string> _createdFiles = new();

    public EnvLoaderTests()
    {
        _testKey = "TEST_ENV_VAR_" + Guid.NewGuid().ToString("N");
        _testValue = "TestValue";
    }

    public void Dispose()
    {
        // Cleanup environment variables
        Environment.SetEnvironmentVariable(_testKey, null);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        
        // Cleanup created files
        foreach (var file in _createdFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    [Fact]
    public void Load_ShouldLoadEnvVariables_FromDevEnv_WhenInDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "dev.env");
        File.WriteAllText(filePath, $"{_testKey}={_testValue}");
        _createdFiles.Add(filePath);

        // Act
        EnvLoader.Load();

        // Assert
        Environment.GetEnvironmentVariable(_testKey).Should().Be(_testValue);
    }

    [Fact]
    public void Load_ShouldLoadEnvVariables_FromProdEnv_WhenInProduction()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "prod.env");
        File.WriteAllText(filePath, $"{_testKey}={_testValue}");
        _createdFiles.Add(filePath);

        // Act
        EnvLoader.Load();

        // Assert
        Environment.GetEnvironmentVariable(_testKey).Should().Be(_testValue);
    }

    [Fact]
    public void Load_ShouldFallbackToExampleDevEnv_WhenDevEnvMissing_InDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        // Ensure dev.env does not exist
        var devPath = Path.Combine(Directory.GetCurrentDirectory(), "dev.env");
        if (File.Exists(devPath)) File.Delete(devPath);

        // Create example.dev.env in parent directory (simulating project root)
        // Note: Unit tests run in bin/Debug/netX.X, so parent is bin/Debug. 
        // EnvLoader checks current and parent.
        // Let's create it in the current directory's parent to test that logic specifically if possible,
        // or just rely on the logic that checks ".."
        
        var parentDir = Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;
        var examplePath = Path.Combine(parentDir, "example.dev.env");
        File.WriteAllText(examplePath, $"{_testKey}={_testValue}");
        _createdFiles.Add(examplePath);

        // Act
        EnvLoader.Load();

        // Assert
        Environment.GetEnvironmentVariable(_testKey).Should().Be(_testValue);
    }
}
