using StudentManagement.Application.Classrooms;
using SortDirection = StudentManagement.Application.Common.SortDirection;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Teachers;
using StudentManagement.Infrastructure.Persistence.Repositories;
using static StudentManagement.Infrastructure.Tests.DatabaseFixture;

namespace StudentManagement.Infrastructure.Tests;

[Collection(DatabaseCollection.Name)]
public class RepositoryQueryTests(DatabaseFixture db) : DatabaseTest(db)
{
    [Fact]
    public async Task Students_sort_by_vietnamese_given_name()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        await Db.SaveAsync(
            NewStudent(classroom, "SV001", "Trần Văn Đức"), NewStudent(classroom, "SV002", "Lê Thị Ánh"),
            NewStudent(classroom, "SV003", "Phạm Giang"), NewStudent(classroom, "SV004", "Võ Minh Dũng"),
            NewStudent(classroom, "SV005", "Bùi Thanh Bình"), NewStudent(classroom, "SV006", "Đỗ An"));

        using var session = Db.OpenSession();
        var repository = new StudentRepository(session);

        var ascending = await repository.SearchAsync(new StudentQuery { SortBy = StudentSortField.Name });
        var descending = await repository.SearchAsync(new StudentQuery
        {
            SortBy = StudentSortField.Name, Direction = SortDirection.Descending,
        });

        string[] expected = ["An", "Ánh", "Bình", "Dũng", "Đức", "Giang"];
        ascending.Items.Select(s => s.GivenName).ShouldBe(expected);
        descending.Items.Select(s => s.GivenName).ShouldBe(expected.Reverse());
    }

    [Fact]
    public async Task Same_given_name_falls_back_to_full_name()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        await Db.SaveAsync(NewStudent(classroom, "SV001", "Trần An"), NewStudent(classroom, "SV002", "Lê An"));

        using var session = Db.OpenSession();
        var page = await new StudentRepository(session).SearchAsync(new StudentQuery());

        page.Items.Select(s => s.FullName).ShouldBe(new[] { "Lê An", "Trần An" });
    }

    [Fact]
    public async Task Students_filter_by_code_prefix_and_classroom_and_page()
    {
        var (teacher, classroom) = await Db.SaveTeacherWithClassroomAsync();
        var other = new Classroom("LH002", "Lớp 10A2", "Văn", teacher);
        await Db.SaveAsync(other,
            NewStudent(classroom, "SV001"), NewStudent(classroom, "SV002"), NewStudent(classroom, "SV003"),
            NewStudent(other, "SV004"), NewStudent(classroom, "SV010"));

        using var session = Db.OpenSession();
        var repository = new StudentRepository(session);

        var byPrefix = await repository.SearchAsync(new StudentQuery
        {
            CodePrefix = "SV00", ClassroomId = classroom.Id, SortBy = StudentSortField.Code, PageSize = 2, Page = 2,
        });

        byPrefix.TotalCount.ShouldBe(3);
        byPrefix.Items.Select(s => s.Code).ShouldBe(new[] { "SV003" });
    }

    [Fact]
    public async Task A_page_of_20_students_with_classroom_and_teacher_takes_at_most_2_statements()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        await Db.SaveAsync(Enumerable.Range(1, 25).Select(i => (object)NewStudent(classroom, $"SV{i:000}")).ToArray());

        var statistics = Db.SessionFactory.Statistics;
        statistics.IsStatisticsEnabled = true;
        statistics.Clear();
        using var session = Db.OpenSession();

        var page = await new StudentRepository(session).SearchAsync(new StudentQuery { PageSize = 20 });
        // Touch what the DTO mapping touches: must not lazy-load anything.
        var teacherNames = page.Items.Select(s => s.Classroom.Teacher.FullName).ToList();

        statistics.IsStatisticsEnabled = false;
        page.TotalCount.ShouldBe(25);
        teacherNames.Count.ShouldBe(20);
        statistics.PrepareStatementCount.ShouldBeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task Code_lookups_are_exact_and_can_exclude_self()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        var student = NewStudent(classroom, "SV001");
        await Db.SaveAsync(student, NewStudent(classroom, "SV0011"));

        using var session = Db.OpenSession();
        var repository = new StudentRepository(session);

        (await repository.GetByCodeAsync("SV001")).ShouldNotBeNull().Id.ShouldBe(student.Id);
        (await repository.GetByCodeAsync("SV00")).ShouldBeNull();
        (await repository.ExistsByCodeAsync("SV001")).ShouldBeTrue();
        (await repository.ExistsByCodeAsync("SV001", excludeId: student.Id)).ShouldBeFalse();
    }

    [Fact]
    public async Task Classrooms_search_by_code_prefix_or_name_and_count_students()
    {
        var (teacher, classroom) = await Db.SaveTeacherWithClassroomAsync();
        var empty = new Classroom("LH002", "Lớp Chuyên Toán", "Toán", teacher);
        await Db.SaveAsync(empty, new Classroom("XY001", "Lớp 12C1", "Hóa", teacher),
            NewStudent(classroom, "SV001"), NewStudent(classroom, "SV002"));

        using var session = Db.OpenSession();
        var repository = new ClassroomRepository(session);

        var byCode = await repository.SearchAsync(new ClassroomQuery { Keyword = "lh" });
        var byName = await repository.SearchAsync(new ClassroomQuery { Keyword = "chuyên" });
        var counts = await repository.CountStudentsAsync([classroom.Id, empty.Id]);

        byCode.Items.Select(c => c.Code).ShouldBe(new[] { "LH001", "LH002" });
        byName.Items.ShouldHaveSingleItem().Code.ShouldBe("LH002");
        counts.ShouldBe(new Dictionary<int, int> { [classroom.Id] = 2 });
        (await repository.CountStudentsAsync([])).ShouldBeEmpty();
    }

    [Fact]
    public async Task Teachers_sort_by_given_name_and_count_classrooms()
    {
        var bao = new Teacher("GV001", "Phạm Quốc Bảo", new DateOnly(1972, 11, 21));
        var anh = new Teacher("GV002", "Lê Minh Anh", new DateOnly(1990, 1, 5));
        await Db.SaveAsync(bao, anh, new Classroom("LH001", "Lớp 10A1", "Toán", bao),
            new Classroom("LH002", "Lớp 10A2", "Lý", bao));

        using var session = Db.OpenSession();
        var repository = new TeacherRepository(session);

        var page = await repository.SearchAsync(new TeacherQuery());
        var byKeyword = await repository.SearchAsync(new TeacherQuery { Keyword = "bảo" });
        var counts = await repository.CountClassroomsAsync([bao.Id, anh.Id]);

        page.Items.Select(t => t.Code).ShouldBe(new[] { "GV002", "GV001" }); // Anh < Bảo
        byKeyword.Items.ShouldHaveSingleItem().Code.ShouldBe("GV001");
        counts.ShouldBe(new Dictionary<int, int> { [bao.Id] = 2 });
    }
}
