using StudentManagement.Application.Classrooms;

namespace StudentManagement.Application.Students;

public sealed record StudentDto(
    int Id, string Code, string FullName, DateOnly DateOfBirth, string? Address, ClassroomSummaryDto Classroom, int Version);

/// <summary>Data entered on the create/edit form.</summary>
public sealed record StudentInput(string Code, string FullName, DateOnly DateOfBirth, string? Address, int ClassroomId);
