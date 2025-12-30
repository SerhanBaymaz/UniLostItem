using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ApiExtensions
{
    public static void AddApiConfiguration(this IServiceCollection services)
    {
        // Add services to the container.
        services.AddControllers();

        // Expose a simple liveness endpoint for container orchestrators
        services.AddHealthChecks();

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

    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors();
    }

    public static void UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
            .WithOrigins("http://localhost:3000", "https://localhost:3000"));
    }

    public static void UseHealthChecksConfiguration(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health");
    }
}
