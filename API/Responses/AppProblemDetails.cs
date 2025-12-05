using System;
using System.Collections.Generic;

namespace API.Responses;

/// <summary>
/// Represents RFC 9457 (Problem Details) compliant error response data for HTTP APIs.
/// This DTO models the standard problem details members (`type`, `title`, `status`,
/// `detail`, `instance`) and supports additional extension members via `Extensions`.
/// Use this type for serializing error responses as `application/problem+json`.
/// </summary>
public class AppProblemDetails
{
    public string Type { get; init; } = "about:blank";

    public string? Title { get; init; }

    public int? Status { get; init; }

    public string? Detail { get; init; }

    public string? Instance { get; init; }

    // RFC 9457 allows extension members. Use a dictionary to carry them.
    public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>();

    public AppProblemDetails() { }

    public AppProblemDetails(int? status, string? detail, string? type, string? title, string? instance)
    {
        Status = status;
        Detail = detail;
        if (!string.IsNullOrEmpty(type))
        {
            Type = type!;
        }
        Title = title;
        Instance = instance;
    }
}
