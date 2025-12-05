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
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
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
            stackTrace
        );

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(standardResponse, options);
        await context.Response.WriteAsync(json);
    }


    private async Task HandleException(HttpContext context, Exception ex)
    {
        logger.LogError(ex, ex.Message);

        // RFC 9457 recommends using the "application/problem+json" media type
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

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
        var standardResponse = StandardApiResponse<object>.ErrorResponse(problemDetails);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(standardResponse, options);
        await context.Response.WriteAsync(json);
    }

    #endregion

}
