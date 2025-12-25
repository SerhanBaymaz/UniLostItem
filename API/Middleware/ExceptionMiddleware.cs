using System;
using System.Text.Json;
using API.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace API.Middleware;

public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);

            // Handle non-exception error responses (e.g. 404, 415)
            if (!context.Response.HasStarted && context.Response.StatusCode >= 400 && context.Response.StatusCode < 600)
            {
                await HandleErrorResponse(context);
            }
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    #region Private Methods - Handle Exceptions


    private static async Task HandleErrorResponse(HttpContext context)
    {
        var statusCode = context.Response.StatusCode;
        context.Response.ContentType = "application/json";

        var message = GetDefaultMessageForStatusCode(statusCode);
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        var response = StandardApiResponse<object>.ErrorResponse(
            type: $"Error {statusCode}",
            message: message,
            statusCode: statusCode,
            traceId: traceId
        );

        var json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }

    private static string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Resource Not Found",
            405 => "Method Not Allowed",
            406 => "Not Acceptable",
            415 => "Unsupported Media Type",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            503 => "Service Unavailable",
            _ => "An error occurred"
        };
    }

    private async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        var validationErrors = new Dictionary<string, string[]>();

        if (ex.Errors is not null)
        {
            foreach (var error in ex.Errors)
            {
                if (validationErrors.TryGetValue(error.PropertyName, out var existingErrors))
                {
                    validationErrors[error.PropertyName] = [.. existingErrors, error.ErrorMessage];
                }
                else
                {
                    validationErrors[error.PropertyName] = [error.ErrorMessage];
                }
            }
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        // Get trace ID
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        // Create stack trace for development mode
        string[]? stackTrace = null;
        if (env.IsDevelopment())
        {
            var dem = ex.Demystify();
            stackTrace = dem.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // Use StandardApiResponse for validation errors
        var standardResponse = StandardApiResponse<object>.ValidationErrorResponse(
            validationErrors,
            context.Request?.Path.ToString(),
            stackTrace,
            traceId
        );

        var json = JsonSerializer.Serialize(standardResponse, JsonOptions);

        // Log validation details
        logger.LogWarning("Validation failed for {Path} with {ErrorCount} errors", context.Request?.Path, validationErrors.Count);

        await context.Response.WriteAsync(json);
    }


    private async Task HandleException(HttpContext context, Exception ex)
    {
        logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request?.Path);

        // RFC 9457 recommends using the "application/problem+json" media type
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Get trace ID
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        // Create stack trace for development mode
        string[]? stackTrace = null;
        if (env.IsDevelopment())
        {
            var dem = ex.Demystify();
            stackTrace = dem.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // Create problem details
        var problemDetails = new AppProblemDetails(
            status: context.Response.StatusCode,
            detail: ex.Message,
            type: "InternalServerError",
            title: "Internal Server Error",
            instance: context.Request?.Path.ToString()
        );

        if (stackTrace != null)
        {
            problemDetails.Extensions["stackTrace"] = stackTrace;
        }

        // Use StandardApiResponse for errors
        var standardResponse = StandardApiResponse<object>.ErrorResponse(
            problemDetails,
            traceId: traceId
        );

        var json = JsonSerializer.Serialize(standardResponse, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    #endregion

}
