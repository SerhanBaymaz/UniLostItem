using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace API.Responses;

[method: JsonConstructor]
/// <summary>
/// Standardized API response wrapper for all HTTP responses.
/// Provides a consistent structure for both successful and error responses.
/// Compatible with RFC 9457 Problem Details for error scenarios.
/// </summary>
/// <typeparam name="T">The type of data being returned in successful responses</typeparam>
public class StandardApiResponse<T>()
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// The actual data payload (only present in successful responses)
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// RFC 9457 Problem Details for error responses (only present when Success is false)
    /// </summary>
    public AppProblemDetails? Error { get; init; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional metadata for pagination, filtering, etc.
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }

    #region Success Response Factory Methods

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static StandardApiResponse<T> SuccessResponse(
        T data,
        string? message = null,
        int statusCode = 200,
        IDictionary<string, object?>? metadata = null)
    {
        return new StandardApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message ?? "Request completed successfully",
            Data = data,
            Error = null,
            Metadata = metadata
        };
    }


    #endregion

    #region Error Response Factory Methods

    /// <summary>
    /// Creates an error response with RFC 9457 Problem Details
    /// </summary>
    public static StandardApiResponse<T> ErrorResponse(
        AppProblemDetails problemDetails,
        string? message = null,
        string? traceId = null)
    {
        if (!string.IsNullOrEmpty(traceId))
        {
            problemDetails.Extensions["traceId"] = traceId;
        }

        return new StandardApiResponse<T>
        {
            Success = false,
            StatusCode = problemDetails.Status ?? 500,
            Message = message ?? problemDetails.Title ?? "An error occurred",
            Data = default,
            Error = problemDetails
        };
    }

    /// <summary>
    /// Creates a simple error response
    /// </summary>
    public static StandardApiResponse<T> ErrorResponse(
        string message,
        int statusCode = 400,
        string? type = null,
        string? detail = null,
        string? instance = null,
        IDictionary<string, object?>? extensions = null,
        string? traceId = null)
    {
        var problemDetails = new AppProblemDetails(
            status: statusCode,
            detail: detail ?? message,
            type: type,
            title: message,
            instance: instance
        );

        if (extensions != null)
        {
            foreach (var ext in extensions)
            {
                problemDetails.Extensions[ext.Key] = ext.Value;
            }
        }

        if (!string.IsNullOrEmpty(traceId))
        {
            problemDetails.Extensions["traceId"] = traceId;
        }

        return new StandardApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Data = default,
            Error = problemDetails
        };
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static StandardApiResponse<T> ValidationErrorResponse(
        Dictionary<string, string[]> validationErrors,
        string? instance = null,
        string[]? stackTrace = null,
        string? traceId = null)
    {
        var problemDetails = new AppProblemDetails(
            status: 400,
            detail: "One or more validation errors has occurred",
            type: "ValidationFailure",
            title: "Validation error",
            instance: instance
        );

        problemDetails.Extensions["errors"] = validationErrors;

        if (stackTrace != null)
        {
            problemDetails.Extensions["stackTrace"] = stackTrace;
        }

        if (!string.IsNullOrEmpty(traceId))
        {
            problemDetails.Extensions["traceId"] = traceId;
        }

        return new StandardApiResponse<T>
        {
            Success = false,
            StatusCode = 400,
            Message = "Validation failed",
            Data = default,
            Error = problemDetails
        };
    }

    #endregion
}
