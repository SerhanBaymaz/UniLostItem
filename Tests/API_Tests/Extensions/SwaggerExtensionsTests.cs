using API.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Tests.API_Tests.Extensions;

public class SwaggerExtensionsTests
{
    [Fact]
    public void AddSwaggerDocumentation_ShouldRegisterSwaggerGen()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerDocumentation();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var swaggerGenOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<SwaggerGenOptions>>();

        swaggerGenOptions.Should().NotBeNull();
    }

    [Fact]
    public void AddSwaggerDocumentation_ShouldRegisterEndpointsApiExplorer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerDocumentation();

        // Assert
        // Check that API explorer services are registered
        var apiExplorerService = services.Any(s => s.ServiceType.Name.Contains("ApiDescription"));
        apiExplorerService.Should().BeTrue();
    }

    [Fact]
    public void AddSwaggerDocumentation_ShouldConfigureSwaggerDoc()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerDocumentation();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<SwaggerGenOptions>>();

        options.Should().NotBeNull();
        var swaggerDocs = options!.Value.SwaggerGeneratorOptions.SwaggerDocs;

        swaggerDocs.Should().ContainKey("v1");
        swaggerDocs["v1"].Title.Should().Be("Uni Lost Item API");
        swaggerDocs["v1"].Version.Should().Be("v1");
    }

    [Fact]
    public void AddSwaggerDocumentation_ShouldConfigureCustomSchemaIds()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerDocumentation();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<SwaggerGenOptions>>();

        options.Should().NotBeNull();
        options!.Value.SchemaGeneratorOptions.SchemaIdSelector.Should().NotBeNull();
    }

    [Fact]
    public void AddSwaggerDocumentation_CustomSchemaIds_ShouldHandleNestedTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSwaggerDocumentation();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<SwaggerGenOptions>>();
        var schemaIdSelector = options!.Value.SchemaGeneratorOptions.SchemaIdSelector;

        // Create a nested type for testing
        var nestedType = typeof(OuterClass.InnerClass);

        // Act
        var schemaId = schemaIdSelector(nestedType);

        // Assert
        schemaId.Should().Contain(".");
        schemaId.Should().NotContain("+");
    }

    [Fact]
    public void AddSwaggerDocumentation_CustomSchemaIds_ShouldUseFullName()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSwaggerDocumentation();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<SwaggerGenOptions>>();
        var schemaIdSelector = options!.Value.SchemaGeneratorOptions.SchemaIdSelector;

        var testType = typeof(SwaggerExtensionsTests);

        // Act
        var schemaId = schemaIdSelector(testType);

        // Assert
        schemaId.Should().Be(testType.FullName?.Replace('+', '.') ?? testType.Name);
    }

    [Fact]
    public void UseSwaggerDocumentation_ShouldNotThrow_InDevelopment()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSwaggerDocumentation();
        services.AddRouting();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        Action act = () => app.UseSwaggerDocumentation(envMock.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UseSwaggerDocumentation_ShouldNotThrow_InProduction()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSwaggerDocumentation();
        services.AddRouting();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var envMock = new Mock<IWebHostEnvironment>();

        // Act
        Action act = () => app.UseSwaggerDocumentation(envMock.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddSwaggerDocumentation_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerDocumentation();
        services.AddSwaggerDocumentation();

        // Assert
        Action act = () => services.BuildServiceProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void UseSwaggerDocumentation_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSwaggerDocumentation();
        services.AddRouting();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");
        // Act
        Action act = () =>
        {
            app.UseSwaggerDocumentation(envMock.Object);
            app.UseSwaggerDocumentation(envMock.Object);
        };

        // Assert
        act.Should().NotThrow();
    }

    // Helper nested class for testing schema ID generation
#pragma warning disable S1144 // Unused private types or members should be removed
    private class OuterClass
    {
        public class InnerClass
        {
#pragma warning disable IDE0051 // Remove unused private members
            public string? TestProperty { get; set; }
#pragma warning restore IDE0051
        }
    }
#pragma warning restore S1144
}
