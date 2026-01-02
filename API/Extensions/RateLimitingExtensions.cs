using System.Threading.RateLimiting;
using API.Responses;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Extensions;

public static class RateLimitingExtensions
{
    public static void AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
        var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);
        var queueLimit = configuration.GetValue("RateLimiting:QueueLimit", 0);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var response = StandardApiResponse<object>.ErrorResponse(
                    message: "Rate limit exceeded. Please try again later.",
                    statusCode: StatusCodes.Status429TooManyRequests,
                    type: "https://tools.ietf.org/html/rfc6585#section-4",
                    detail: "You have sent too many requests in a given amount of time."
                );

                await context.HttpContext.Response.WriteAsJsonAsync(response, token);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = permitLimit,
                        QueueLimit = queueLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds)
                    }));
        });
    }
}
