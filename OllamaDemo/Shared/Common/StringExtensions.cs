using System.Text.RegularExpressions;

namespace OllamaDemo.Shared.Common;

public static partial class StringExtensions
{
    public static string ReplacePlaceholders(this string text, IReadOnlyDictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(text)) { return text; }
        return PlaceholderRegex.Replace(text, match =>
        {
            var key = match.Groups[1].Value.Trim();
            if (values.TryGetValue(key, out var value))
            {
                return value;
            }
            return match.Value;
        });
    }

    [GeneratedRegex(@"\{([^}]+)\}")]
    private static partial Regex PlaceholderRegex { get; }
}
