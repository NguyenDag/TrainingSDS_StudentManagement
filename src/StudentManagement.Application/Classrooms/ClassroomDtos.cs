using StudentManagement.Application.Teachers;

namespace StudentManagement.Application.Classrooms;

public sealed record ClassroomSummaryDto(int Id, string Code, string Name, string Subject, TeacherSummaryDto Teacher);

public sealed record ClassroomDto(
    int Id, string Code, string Name, string Subject, TeacherSummaryDto Teacher, int StudentCount, int Version);

/// <summary>Data entered on the create/edit form.</summary>
public sealed record ClassroomInput(string Code, string Name, string Subject, int TeacherId);
