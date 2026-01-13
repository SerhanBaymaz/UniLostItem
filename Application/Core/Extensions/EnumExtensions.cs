using System.ComponentModel;
using System.Reflection;

namespace Application.Core.Extensions;

/// <summary>
/// Extension methods for enum types
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description attribute value from an enum value
    /// </summary>
    public static string? GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description;
    }
}
