using API.Extensions;
using Application.Core;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.API_Tests.Extensions;

public class ApplicationExtensionsTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterMediatR()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterAutoMapper()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        mapper.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterValidators()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        // Check that FluentValidation assembly scanning registered validators
        var validatorRegistrations = services.Where(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition().Name.Contains("IValidator"));

        validatorRegistrations.Should().NotBeEmpty();
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterCreateSerhanKitapCommandValidator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetService<IValidator<CreateSerhanKitapCommand>>();

        validator.Should().NotBeNull();
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterValidationBehavior()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        // ValidationBehavior is registered as an open generic pipeline behavior
        var pipelineBehaviors = services.Where(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        pipelineBehaviors.Should().NotBeEmpty();
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterMappingProfiles()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        mapper.Should().NotBeNull();

        // Test that mapper configuration is valid
        Action act = () => mapper!.ConfigurationProvider.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddApplicationServices_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        services.AddApplicationServices();

        // Assert
        Action act = () => services.BuildServiceProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        // Check that request handlers are registered
        var requestHandlers = services.Where(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        requestHandlers.Should().NotBeEmpty();
    }
}
