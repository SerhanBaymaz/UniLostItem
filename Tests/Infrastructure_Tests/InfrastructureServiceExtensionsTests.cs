using Application.Interfaces;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.Infrastructure_Tests;

public class InfrastructureServiceExtensionsTests
{
    [Fact]
    public void AddInfrastructureServices_ShouldRegisterExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();

        // Account details for CloudinaryImageStorageService constructor
        configurationMock.Setup(x => x["Cloudinary:CloudName"]).Returns("test-cloud");
        configurationMock.Setup(x => x["Cloudinary:ApiKey"]).Returns("test-key");
        configurationMock.Setup(x => x["Cloudinary:ApiSecret"]).Returns("test-secret");

        // Register IConfiguration in the service collection for dependent services (like JwtService)
        services.AddSingleton(configurationMock.Object);

        // Add HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();

        // Mock ILogger for CloudinaryImageStorageService
        services.AddLogging();

        // Act
        services.AddInfrastructureServices(configurationMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IJwtService>().Should().BeOfType<JwtService>();
        serviceProvider.GetService<ICurrentUserService>().Should().BeOfType<CurrentUserService>();
        serviceProvider.GetService<IImageStorageService>().Should().BeOfType<CloudinaryImageStorageService>();
    }
}
