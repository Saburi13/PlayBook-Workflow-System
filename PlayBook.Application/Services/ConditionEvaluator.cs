using System.Globalization;
using System.Text.Json;
using PlayBook.Application.Interfaces;

namespace PlayBook.Application.Services;

public class ConditionEvaluator : IConditionEvaluator
{
    public bool Evaluate(string field, string @operator, string? value, object? model, string? dataType = "string")
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return false;
        }

        var propertyValue = GetPropertyValue(model, field);
        if (propertyValue == null)
        {
            return @operator switch
            {
                "IsNull" => true,
                "IsNotNull" => false,
                _ => false
            };
        }

        return @operator switch
        {
            "Equals" => CompareEquals(propertyValue, value),
            "NotEquals" => !CompareEquals(propertyValue, value),
            "GreaterThan" => CompareGreaterThan(propertyValue, value),
            "LessThan" => CompareLessThan(propertyValue, value),
            "GreaterThanOrEqual" => CompareGreaterThan(propertyValue, value) || CompareEquals(propertyValue, value),
            "LessThanOrEqual" => CompareLessThan(propertyValue, value) || CompareEquals(propertyValue, value),
            "Contains" => propertyValue.ToString()?.Contains(value ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "StartsWith" => propertyValue.ToString()?.StartsWith(value ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "EndsWith" => propertyValue.ToString()?.EndsWith(value ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "IsNull" => false,
            "IsNotNull" => true,
            _ => false
        };
    }

    private static object? GetPropertyValue(object? model, string field)
    {
        if (model == null)
        {
            return null;
        }

        var current = model;
        foreach (var segment in field.Split('.'))
        {
            if (current == null)
            {
                return null;
            }

            if (current is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind != JsonValueKind.Object || !jsonElement.TryGetProperty(segment, out var jsonProperty))
                {
                    var matchingProperty = jsonElement.EnumerateObject().FirstOrDefault(property => string.Equals(property.Name, segment, StringComparison.OrdinalIgnoreCase));
                    if (matchingProperty.Equals(default(JsonProperty))) return null;
                    jsonProperty = matchingProperty.Value;
                }

                current = jsonProperty.ValueKind == JsonValueKind.Null ? null : jsonProperty;
                continue;
            }

            var propertyInfo = current.GetType().GetProperty(segment);
            if (propertyInfo == null) return null;
            current = propertyInfo.GetValue(current);
        }

        return current;
    }

    private static bool CompareEquals(object propertyValue, string? value)
    {
        return string.Equals(propertyValue.ToString(), value, StringComparison.OrdinalIgnoreCase);
    }

    private static bool CompareGreaterThan(object propertyValue, string? value)
    {
        if (decimal.TryParse(propertyValue.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var left) &&
            decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var right))
        {
            return left > right;
        }

        return string.Compare(propertyValue.ToString(), value, StringComparison.OrdinalIgnoreCase) > 0;
    }

    private static bool CompareLessThan(object propertyValue, string? value)
    {
        if (decimal.TryParse(propertyValue.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var left) &&
            decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var right))
        {
            return left < right;
        }

        return string.Compare(propertyValue.ToString(), value, StringComparison.OrdinalIgnoreCase) < 0;
    }
}
