using System.Text.RegularExpressions;
using StudentManagement.Domain.Exceptions;

namespace StudentManagement.Domain.Common;

internal static partial class Guard
{
    public static string Code(string? value)
    {
        var code = EntityCode.Normalize(value);
        if (!EntityCode.IsValid(code))
            throw new DomainException($"Code must be 3-20 letters (A-Z) or digits, got \"{value}\".");
        return code;
    }

    public static string RequiredText(string? value, string field, int maxLength) =>
        OptionalText(value, field, maxLength) ?? throw new DomainException($"{field} is required.");

    /// <summary>Trims and collapses inner whitespace; blank input becomes null.</summary>
    public static string? OptionalText(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = Whitespace().Replace(value.Trim(), " ");
        if (text.Length > maxLength)
            throw new DomainException($"{field} must be at most {maxLength} characters.");
        return text;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
