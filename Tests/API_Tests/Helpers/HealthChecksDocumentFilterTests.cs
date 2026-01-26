using API.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tests.API_Tests.Helpers;

public class HealthChecksDocumentFilterTests
{
    [Fact]
    public void Apply_ShouldAddHealthCheckPaths_WhenNotPresent()
    {
        // Arrange
        var filter = new HealthChecksDocumentFilter();
        var swaggerDoc = new OpenApiDocument
        {
            Paths = []
        };

        // Mock ISchemaGenerator as it's required for DocumentFilterContext but not used in our filter
        var schemaGeneratorMock = new Mock<ISchemaGenerator>();
        var context = new DocumentFilterContext(
            new List<ApiDescription>(),
            schemaGeneratorMock.Object,
            new SchemaRepository()
        );

        // Act
        filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Paths.Should().ContainKey("/health/api");
        swaggerDoc.Paths.Should().ContainKey("/health/all");

        // Verify /health/api (Liveness)
        var livenessPath = swaggerDoc.Paths["/health/api"];
        livenessPath.Operations.Should().ContainKey(OperationType.Get);

        var livenessOp = livenessPath.Operations[OperationType.Get];
        livenessOp.Tags.Should().Contain(t => t.Name == "Health");
        livenessOp.Responses.Should().ContainKey("200");
        livenessOp.Responses.Should().ContainKey("503");
        livenessOp.Summary.Should().Be("API Liveness Check");

        // Verify /health/all (Readiness)
        var readinessPath = swaggerDoc.Paths["/health/all"];
        readinessPath.Operations.Should().ContainKey(OperationType.Get);

        var readinessOp = readinessPath.Operations[OperationType.Get];
        readinessOp.Tags.Should().Contain(t => t.Name == "Health");
        readinessOp.Responses.Should().ContainKey("200");
        readinessOp.Responses.Should().ContainKey("503");
        readinessOp.Summary.Should().Be("API Readiness Check");
    }

    [Fact]
    public void Apply_ShouldThrowException_WhenDuplicateKeyExists()
    {
        // This test ensures we understand behavior if paths already exist.
        // Since the current implementation uses .Add(), it should throw ArgumentException if key exists.

        // Arrange
        var filter = new HealthChecksDocumentFilter();
        var swaggerDoc = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                { "/health/api", new OpenApiPathItem() }
            }
        };

        var schemaGeneratorMock = new Mock<ISchemaGenerator>();
        var context = new DocumentFilterContext(
            new List<ApiDescription>(),
            schemaGeneratorMock.Object,
            new SchemaRepository()
        );

        // Act
        var act = () => filter.Apply(swaggerDoc, context);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("An item with the same key has already been added. Key: /health/api");
    }
}
