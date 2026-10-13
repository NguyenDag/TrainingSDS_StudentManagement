namespace StudentManagement.Application.Teachers;

public sealed record TeacherSummaryDto(int Id, string Code, string FullName);

public sealed record TeacherDto(int Id, string Code, string FullName, DateOnly DateOfBirth, int ClassroomCount, int Version);

/// <summary>Data entered on the create/edit form.</summary>
public sealed record TeacherInput(string Code, string FullName, DateOnly DateOfBirth);
