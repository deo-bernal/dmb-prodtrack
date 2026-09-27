namespace ProdTrack.Domain.Common;

internal static class Guard
{
    public static string Required(string? value, int maxLength, string field, List<KeyValuePair<string, string>> errors)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            errors.Add(new(field, $"{field} is required."));
        }
        else if (trimmed.Length > maxLength)
        {
            errors.Add(new(field, $"{field} must be at most {maxLength} characters."));
        }

        return trimmed;
    }

    public static ValidationError ToValidationError(this List<KeyValuePair<string, string>> errors) =>
        new(errors.GroupBy(e => e.Key, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Value).ToArray(), StringComparer.Ordinal));
}
