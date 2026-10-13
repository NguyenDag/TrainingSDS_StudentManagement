using StudentManagement.Application.Common;

namespace StudentManagement.Application.Teachers;

public enum TeacherSortField
{
    /// <summary>Vietnamese given name (last word), then full name.</summary>
    Name,
    Code,
    DateOfBirth,
}

public sealed record TeacherQuery : PagedQuery
{
    public TeacherSortField SortBy { get; init; }

    /// <summary>Matches a code prefix or part of the full name.</summary>
    public string? Keyword { get; init; }
}
