using StudentManagement.Application.Common;

namespace StudentManagement.Application.Classrooms;

public enum ClassroomSortField
{
    Code,
    Name,
    Subject,
}

public sealed record ClassroomQuery : PagedQuery
{
    public ClassroomSortField SortBy { get; init; }

    /// <summary>Matches a code prefix or part of the classroom name.</summary>
    public string? Keyword { get; init; }

    public int? TeacherId { get; init; }
}
