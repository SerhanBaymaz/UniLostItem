using Serilog;
using System.Diagnostics;

namespace API.Extensions;

public static class LoggingExtensions
{
    public static void AddSerilogConfiguration(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });
    }

    public static void UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            // Prefer Activity.TraceId (distributed tracing) when available
            var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
            diagnosticContext.Set("TraceId", traceId);
            // Also include the W3C traceparent (includes version/trace/span/flags) so it matches API responses
            var traceParent = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            diagnosticContext.Set("TraceParent", traceParent);
        });
    }
}
