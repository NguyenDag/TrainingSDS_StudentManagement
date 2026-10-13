namespace StudentManagement.Application.Reports;

/// <summary>One bar/slice of a chart.</summary>
public sealed record CountByLabel(string Label, int Count);

/// <summary>Aggregates exactly as the database computes them (GROUP BY), before any shaping for display.</summary>
/// <param name="StudentsByAge">Age in whole years → number of students.</param>
public sealed record ReportData(
    int TotalStudents,
    int TotalClassrooms,
    int TotalTeachers,
    IReadOnlyList<CountByLabel> StudentsByClassroom,
    IReadOnlyList<CountByLabel> StudentsBySubject,
    IReadOnlyDictionary<int, int> StudentsByAge,
    IReadOnlyList<CountByLabel> ClassroomsByTeacher);

public sealed record DashboardDto(
    int TotalStudents,
    int TotalClassrooms,
    int TotalTeachers,
    IReadOnlyList<CountByLabel> StudentsByClassroom,
    IReadOnlyList<CountByLabel> StudentsBySubject,
    IReadOnlyList<CountByLabel> StudentsByAgeGroup,
    IReadOnlyList<CountByLabel> ClassroomsByTeacher);
