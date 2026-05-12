using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Infrastructure_Tests.Services;

public class CloudinaryImageStorageServiceTests
{
    private readonly Mock<ILogger<CloudinaryImageStorageService>> _loggerMock;

    public CloudinaryImageStorageServiceTests()
    {
        _loggerMock = new Mock<ILogger<CloudinaryImageStorageService>>();
    }

    private static IConfiguration BuildConfiguration(string? cloudName, string? apiKey, string? apiSecret)
    {
        var inMemorySettings = new Dictionary<string, string?>();

        if (cloudName != null)
        {
            inMemorySettings["Cloudinary:CloudName"] = cloudName;
        }

        if (apiKey != null)
        {
            inMemorySettings["Cloudinary:ApiKey"] = apiKey;
        }

        if (apiSecret != null)
        {
            inMemorySettings["Cloudinary:ApiSecret"] = apiSecret;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCloudNameIsMissing()
    {
        var config = BuildConfiguration(null, "key", "secret");

        var act = () => new CloudinaryImageStorageService(config, _loggerMock.Object);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CloudName*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenApiKeyIsMissing()
    {
        var config = BuildConfiguration("cloud", null, "secret");

        var act = () => new CloudinaryImageStorageService(config, _loggerMock.Object);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ApiKey*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenApiSecretIsMissing()
    {
        var config = BuildConfiguration("cloud", "key", null);

        var act = () => new CloudinaryImageStorageService(config, _loggerMock.Object);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ApiSecret*");
    }

    [Fact]
    public void Constructor_ShouldSucceed_WhenAllConfigValuesProvided()
    {
        var config = BuildConfiguration("cloud", "key", "secret");

        var act = () => new CloudinaryImageStorageService(config, _loggerMock.Object);

        act.Should().NotThrow();
    }
}
