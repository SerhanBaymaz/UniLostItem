using API.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace API.Helpers;

public static class ModelStateResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = ExtractErrorsFromModelState(context);
        var instance = context.HttpContext.Request?.Path.ToString();
        var traceId = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;

        var standard = StandardApiResponse<object>.ValidationErrorResponse(
            errors,
            instance,
            stackTrace: null,
            traceId: traceId
        );

        var result = new BadRequestObjectResult(standard);
        result.ContentTypes.Add("application/json");
        return result;
    }

    private static Dictionary<string, string[]> ExtractErrorsFromModelState(ActionContext context)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var kvp in context.ModelState)
        {
            var key = GetErrorKey(kvp.Key);
            var errorMessages = GetErrorMessages(kvp.Value);

            if (errorMessages.Length > 0)
            {
                errors[key] = errorMessages;
            }
        }

        return errors;
    }

    private static string GetErrorKey(string key)
    {
        return string.IsNullOrEmpty(key) ? "$" : key;
    }

    private static string[] GetErrorMessages(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry entry)
    {
        return entry.Errors
            .Select(e => GetSingleErrorMessage(e))
            .ToArray();
    }

    private static string GetSingleErrorMessage(Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error)
    {
        return string.IsNullOrEmpty(error.ErrorMessage)
            ? error.Exception?.Message ?? "Invalid value"
            : error.ErrorMessage;
    }
}
