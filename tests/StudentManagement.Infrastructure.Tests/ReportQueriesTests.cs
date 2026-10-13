using StudentManagement.Application.Reports;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;
using StudentManagement.Infrastructure.Persistence;

namespace StudentManagement.Infrastructure.Tests;

[Collection(DatabaseCollection.Name)]
public class ReportQueriesTests(DatabaseFixture db) : DatabaseTest(db)
{
    private static DateOnly YearsAgo(int years) => DateOnly.FromDateTime(DateTime.Today).AddYears(-years);

    [Fact]
    public async Task Aggregates_match_the_data_including_empty_groups()
    {
        var binh = new Teacher("GV001", "Trần Thị Bình", new DateOnly(1980, 5, 1));
        var an = new Teacher("GV002", "Lê Văn An", new DateOnly(1985, 1, 1)); // no classrooms
        var math = new Classroom("LH001", "Lớp 10A1", "Toán", binh);
        var literature = new Classroom("LH002", "Lớp 10A2", "Ngữ văn", binh);
        var empty = new Classroom("LH003", "Lớp 10A3", "Toán", binh); // no students
        await Db.SaveAsync(binh, an, math, literature, empty,
            new Student("SV001", "Nguyễn Văn A", YearsAgo(17), null, math),
            new Student("SV002", "Nguyễn Văn B", YearsAgo(20), null, math),
            new Student("SV003", "Nguyễn Văn C", YearsAgo(30).AddDays(-1), null, literature));

        using var session = Db.OpenSession();
        var data = await new ReportQueries(session).GetAsync();

        (data.TotalStudents, data.TotalClassrooms, data.TotalTeachers).ShouldBe((3, 3, 2));
        data.StudentsByClassroom.ShouldBe(new CountByLabel[] { new("Lớp 10A1", 2), new("Lớp 10A2", 1), new("Lớp 10A3", 0) });
        data.StudentsBySubject.ShouldBe(new CountByLabel[] { new("Toán", 2), new("Ngữ văn", 1) });
        data.StudentsByAge.OrderBy(a => a.Key).ShouldBe(new Dictionary<int, int> { [17] = 1, [20] = 1, [30] = 1 });
        // Ordered by Vietnamese given name: An < Bình.
        data.ClassroomsByTeacher.ShouldBe(new CountByLabel[] { new("Lê Văn An", 0), new("Trần Thị Bình", 3) });
    }

    [Fact]
    public async Task Empty_database_gives_zeros_not_errors()
    {
        using var session = Db.OpenSession();
        var data = await new ReportQueries(session).GetAsync();

        (data.TotalStudents, data.TotalClassrooms, data.TotalTeachers).ShouldBe((0, 0, 0));
        data.StudentsByClassroom.ShouldBeEmpty();
        data.StudentsByAge.ShouldBeEmpty();
    }
}
