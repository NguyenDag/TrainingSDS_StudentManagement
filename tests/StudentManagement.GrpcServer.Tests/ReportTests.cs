using StudentManagement.Contracts.V1;

namespace StudentManagement.GrpcServer.Tests;

[Collection(GrpcServerCollection.Name)]
public class ReportTests(GrpcServerFixture fixture) : GrpcTest(fixture)
{
    [Fact]
    public async Task Dashboard_returns_totals_and_every_chart_series()
    {
        var teacher = await CreateTeacherAsync();
        var classroom = await CreateClassroomAsync(teacher.Id);
        await CreateClassroomAsync(teacher.Id, "LH002");
        await CreateStudentAsync(classroom.Id, "SV001");
        await CreateStudentAsync(classroom.Id, "SV002");

        var dashboard = await new ReportService.ReportServiceClient(App.CreateChannel())
            .GetDashboardAsync(new GetDashboardRequest());

        (dashboard.TotalStudents, dashboard.TotalClassrooms, dashboard.TotalTeachers).ShouldBe((2, 2, 1));
        dashboard.StudentsByClassroom.Select(c => c.Count).ShouldBe(new[] { 2, 0 });
        dashboard.StudentsBySubject.ShouldHaveSingleItem().ShouldBe(new CountByLabel { Label = "Toán", Count = 2 });
        dashboard.StudentsByAgeGroup.Select(g => g.Label).ShouldBe(new[] { "16–18", "19–21", "22–25", ">25" });
        dashboard.StudentsByAgeGroup.Sum(g => g.Count).ShouldBe(2);
        dashboard.ClassroomsByTeacher.ShouldHaveSingleItem().Count.ShouldBe(2);
    }
}
