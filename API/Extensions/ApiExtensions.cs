using API.Helpers;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;

namespace API.Extensions;

public static class ApiExtensions
{
    public static void AddApiConfiguration(this IServiceCollection services)
    {
        // Add services to the container.
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Convert model state / input formatter errors into StandardApiResponse
        services.Configure<ApiBehaviorOptions>(options =>
        {
            // disable the default automatic 400 so our factory runs
            options.SuppressModelStateInvalidFilter = true;
            options.InvalidModelStateResponseFactory = ModelStateResponseFactory.Create;

            // Suppress default ProblemDetails for client errors (4xx) so ExceptionMiddleware can handle them
            options.SuppressMapClientErrors = true;
        });
    }

    public static void AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks()
            .AddCheck("API", () => HealthCheckResult.Healthy(), tags: ["api"]);

        var connectionString = configuration["ConnectionStrings:DefaultConnection"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecks.AddNpgSql(connectionString, name: "PostgreSQL", tags: ["dependency", "postgresql"]);
        }

        var seqUrl = configuration["Serilog:WriteTo:1:Args:serverUrl"];
        if (!string.IsNullOrEmpty(seqUrl) && Uri.TryCreate(seqUrl, UriKind.Absolute, out var uri))
        {
            // Check if Seq ingestion port is open (TCP) because HTTP root might return 404
            healthChecks.AddTcpHealthCheck(setup => setup.AddHost(uri.Host, uri.Port), name: "Seq", tags: ["dependency", "seq"]);
        }
    }

    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors();
    }

    public static void UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
            .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173"));
    }

    public static void UseHealthChecksConfiguration(this IApplicationBuilder app)
    {
        // API only (liveness)
        app.UseHealthChecks("/health/api", new HealthCheckOptions
        {
            Predicate = check => !check.Tags.Contains("dependency"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // All dependencies (readiness)
        app.UseHealthChecks("/health/all", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }
}
