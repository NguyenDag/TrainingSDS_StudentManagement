using StudentManagement.Application.Reports;

namespace StudentManagement.Application.Tests.Services;

public class ReportAppServiceTests
{
    private sealed class FixedReportQueries(ReportData data) : IReportQueries
    {
        public Task<ReportData> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(data);
    }

    private static ReportData Data(Dictionary<int, int> studentsByAge) =>
        new(10, 3, 2, [new("Lớp 10A1", 6)], [new("Toán", 6)], studentsByAge, [new("Trần Thị Bình", 3)]);

    [Fact]
    public async Task Ages_are_grouped_into_fixed_buckets()
    {
        var service = new ReportAppService(new FixedReportQueries(Data(new()
        {
            [15] = 1, [16] = 2, [18] = 1, // 16–18 (15 falls into the first bucket)
            [19] = 3,                     // 19–21
            [22] = 1, [25] = 1,           // 22–25
            [26] = 2, [40] = 1,           // >25
        })));

        var dashboard = await service.GetDashboardAsync();

        dashboard.StudentsByAgeGroup.ShouldBe(new CountByLabel[]
        {
            new("16–18", 4), new("19–21", 3), new("22–25", 2), new(">25", 3),
        });
    }

    [Fact]
    public async Task Empty_buckets_are_still_shown_and_other_figures_pass_through()
    {
        var dashboard = await new ReportAppService(new FixedReportQueries(Data([]))).GetDashboardAsync();

        dashboard.StudentsByAgeGroup.Select(g => g.Count).ShouldBe(new[] { 0, 0, 0, 0 });
        (dashboard.TotalStudents, dashboard.TotalClassrooms, dashboard.TotalTeachers).ShouldBe((10, 3, 2));
        dashboard.StudentsByClassroom.ShouldHaveSingleItem().ShouldBe(new CountByLabel("Lớp 10A1", 6));
        dashboard.ClassroomsByTeacher.ShouldHaveSingleItem().Count.ShouldBe(3);
    }
}
