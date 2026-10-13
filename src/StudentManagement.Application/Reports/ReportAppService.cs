namespace StudentManagement.Application.Reports;

/// <summary>Dashboard use case.</summary>
public interface IReportAppService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}

public sealed class ReportAppService(IReportQueries queries) : IReportAppService
{
    // Age chart buckets, by inclusive upper bound. The first also takes anyone younger (data entered
    // before a rule change); the last is open-ended. Every bucket is shown, even when empty.
    private static readonly (string Label, int MaxAge)[] AgeGroups =
        [("16–18", 18), ("19–21", 21), ("22–25", 25), (">25", int.MaxValue)];

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var data = await queries.GetAsync(cancellationToken);
        return new DashboardDto(
            data.TotalStudents,
            data.TotalClassrooms,
            data.TotalTeachers,
            data.StudentsByClassroom,
            data.StudentsBySubject,
            GroupByAge(data.StudentsByAge),
            data.ClassroomsByTeacher);
    }

    private static List<CountByLabel> GroupByAge(IReadOnlyDictionary<int, int> studentsByAge)
    {
        var groups = new List<CountByLabel>();
        var lowerBound = int.MinValue;
        foreach (var (label, maxAge) in AgeGroups)
        {
            var count = studentsByAge.Where(a => a.Key > lowerBound && a.Key <= maxAge).Sum(a => a.Value);
            groups.Add(new CountByLabel(label, count));
            lowerBound = maxAge;
        }
        return groups;
    }
}
