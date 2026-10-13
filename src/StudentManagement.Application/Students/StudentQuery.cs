using StudentManagement.Application.Common;

namespace StudentManagement.Application.Students;

public enum StudentSortField
{
    /// <summary>Vietnamese given name (last word), then full name.</summary>
    Name,
    Code,
    DateOfBirth,
}

public sealed record StudentQuery : PagedQuery
{
    public StudentSortField SortBy { get; init; }

    public string? CodePrefix { get; init; }

    public int? ClassroomId { get; init; }
}
