using Serilog;
using System.Diagnostics;
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

            options.OnRejected = (context, token) => HandleRejected(context.HttpContext, token, permitLimit, windowSeconds);

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

    public static async ValueTask HandleRejected(HttpContext httpContext, CancellationToken token, int permitLimit, int windowSeconds)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        var clientIPv4 = remoteIp?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
            ? remoteIp.MapToIPv4().ToString()
            : remoteIp?.ToString();
        var clientIPv6 = remoteIp?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            ? remoteIp.MapToIPv6().ToString()
            : remoteIp?.ToString();
        var path = httpContext.Request.Path.Value ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        Log.Warning(
            "Rate limit exceeded for client - ClientIPv4: {ClientIPv4}, ClientIPv6: {ClientIPv6} - Path: {Path} - UserAgent: {UserAgent} - TraceId: {TraceId} - PermitLimit: {PermitLimit} - WindowSeconds: {WindowSeconds}",
            clientIPv4 ?? "N/A", clientIPv6 ?? "N/A", path, userAgent, traceId, permitLimit, windowSeconds);

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json";

        var response = StandardApiResponse<object>.ErrorResponse(
            message: "Rate limit exceeded. Please try again later.",
            statusCode: StatusCodes.Status429TooManyRequests,
            type: "https://tools.ietf.org/html/rfc6585#section-4",
            detail: "You have sent too many requests in a given amount of time."
        );

        await httpContext.Response.WriteAsJsonAsync(response, token);
    }
}
