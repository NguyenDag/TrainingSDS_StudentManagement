using System.Text.RegularExpressions;

namespace StudentManagement.Domain.Common;

/// <summary>Rules for the business code shared by students, classrooms and teachers.</summary>
public static partial class EntityCode
{
    public static string Normalize(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;

    /// <summary>Expects a normalized code. Mirrors the ck_*_code CHECK constraint in the database.</summary>
    public static bool IsValid(string normalizedCode) => Pattern().IsMatch(normalizedCode);

    [GeneratedRegex("^[A-Z0-9]{3,20}$")]
    private static partial Regex Pattern();
}
