using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Helpers;

public class HealthChecksDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var livenessPath = new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Description = "Checks if the API is alive (Liveness)",
                    Summary = "API Liveness Check",
                    Tags = [new OpenApiTag { Name = "Health" }],
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Healthy" },
                        ["503"] = new OpenApiResponse { Description = "Unhealthy" }
                    }
                }
            }
        };

        var readinessPath = new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Description = "Checks if the API and dependencies are ready (Readiness)",
                    Summary = "API Readiness Check",
                    Tags = [new OpenApiTag { Name = "Health" }],
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Healthy" },
                        ["503"] = new OpenApiResponse { Description = "Unhealthy" }
                    }
                }
            }
        };

        swaggerDoc.Paths.Add("/health/api", livenessPath);
        swaggerDoc.Paths.Add("/health/all", readinessPath);
    }
}
