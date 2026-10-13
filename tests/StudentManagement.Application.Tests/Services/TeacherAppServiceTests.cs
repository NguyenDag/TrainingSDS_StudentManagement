using FluentValidation;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Teachers;
using StudentManagement.Application.Tests.Fakes;
using static StudentManagement.Application.Tests.Fakes.TestDb;

namespace StudentManagement.Application.Tests.Services;

public class TeacherAppServiceTests
{
    private readonly TestDb _db = new();
    private readonly TeacherAppService _service;

    public TeacherAppServiceTests() => _service = _db.TeacherService();

    private static TeacherInput Input(string code = "GV100", string fullName = "Lê Văn Cường", int age = 30) =>
        new(code, fullName, YearsAgo(age));

    [Fact]
    public async Task List_returns_page_with_classroom_counts()
    {
        var busy = _db.AddTeacher("GV001");
        _db.AddTeacher("GV002");
        _db.AddClassroom(busy, "LH001");
        _db.AddClassroom(busy, "LH002");

        var page = await _service.ListAsync(new TeacherQuery());

        page.TotalCount.ShouldBe(2);
        page.Items.Select(t => (t.Code, t.ClassroomCount)).ShouldBe(new[] { ("GV001", 2), ("GV002", 0) });
    }

    [Fact]
    public async Task Get_returns_teacher_with_classroom_count()
    {
        var teacher = _db.AddTeacher();
        _db.AddClassroom(teacher);

        var dto = await _service.GetAsync(teacher.Id);

        dto.ShouldBe(new TeacherDto(teacher.Id, "GV001", "Trần Thị Bình", teacher.DateOfBirth, 1, 1));
    }

    [Fact]
    public async Task Get_unknown_id_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.GetAsync(99)))
            .ErrorCode.ShouldBe(ErrorCodes.TeacherNotFound);
    }

    [Fact]
    public async Task Create_stores_normalized_teacher_and_commits()
    {
        var dto = await _service.CreateAsync(Input(code: " gv100 ", fullName: "  Lê   Văn Cường "));

        dto.Code.ShouldBe("GV100");
        dto.FullName.ShouldBe("Lê Văn Cường");
        dto.ClassroomCount.ShouldBe(0);
        (await _db.Teachers.GetAsync(dto.Id)).ShouldNotBeNull();
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Create_with_invalid_input_throws_validation_and_stores_nothing()
    {
        await Should.ThrowAsync<ValidationException>(() => _service.CreateAsync(Input(age: 17)));

        _db.Teachers.Items.ShouldBeEmpty();
        _db.UnitOfWork.Commits.ShouldBe(0);
    }

    [Fact]
    public async Task Create_with_taken_code_throws_conflict()
    {
        _db.AddTeacher("GV001");

        (await Should.ThrowAsync<ConflictException>(() => _service.CreateAsync(Input(code: "gv001"))))
            .ErrorCode.ShouldBe(ErrorCodes.TeacherCodeDuplicated);
        _db.UnitOfWork.Commits.ShouldBe(0);
    }

    [Fact]
    public async Task Update_applies_changes_keeps_own_code_and_bumps_version()
    {
        var teacher = _db.AddTeacher("GV001");
        _db.AddClassroom(teacher);

        var dto = await _service.UpdateAsync(teacher.Id, version: 1, Input(code: "GV001", fullName: "Trần Thị Bích"));

        dto.FullName.ShouldBe("Trần Thị Bích");
        dto.ClassroomCount.ShouldBe(1);
        dto.Version.ShouldBe(2);
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Update_with_invalid_input_throws_validation()
    {
        var teacher = _db.AddTeacher();
        await Should.ThrowAsync<ValidationException>(() => _service.UpdateAsync(teacher.Id, 1, Input(fullName: " ")));
    }

    [Fact]
    public async Task Update_unknown_id_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.UpdateAsync(99, 1, Input())))
            .ErrorCode.ShouldBe(ErrorCodes.TeacherNotFound);
    }

    [Fact]
    public async Task Update_with_stale_version_throws_concurrency()
    {
        var teacher = _db.AddTeacher();

        (await Should.ThrowAsync<ConcurrencyException>(() => _service.UpdateAsync(teacher.Id, version: 0, Input())))
            .ErrorCode.ShouldBe(ErrorCodes.ConcurrencyConflict);
        _db.UnitOfWork.Commits.ShouldBe(0);
    }

    [Fact]
    public async Task Update_to_code_of_another_teacher_throws_conflict()
    {
        _db.AddTeacher("GV001");
        var other = _db.AddTeacher("GV002");

        (await Should.ThrowAsync<ConflictException>(() => _service.UpdateAsync(other.Id, 1, Input(code: "GV001"))))
            .ErrorCode.ShouldBe(ErrorCodes.TeacherCodeDuplicated);
    }

    [Fact]
    public async Task Delete_removes_teacher_and_commits()
    {
        var teacher = _db.AddTeacher();

        await _service.DeleteAsync(teacher.Id);

        (await _db.Teachers.GetAsync(teacher.Id)).ShouldBeNull();
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Delete_unknown_id_throws_not_found()
    {
        await Should.ThrowAsync<NotFoundException>(() => _service.DeleteAsync(99));
    }

    [Fact]
    public async Task Delete_teacher_with_classrooms_throws_in_use_and_keeps_teacher()
    {
        var teacher = _db.AddTeacher();
        _db.AddClassroom(teacher);

        (await Should.ThrowAsync<InUseException>(() => _service.DeleteAsync(teacher.Id)))
            .ErrorCode.ShouldBe(ErrorCodes.TeacherHasClassrooms);
        (await _db.Teachers.GetAsync(teacher.Id)).ShouldNotBeNull();
        _db.UnitOfWork.Commits.ShouldBe(0);
    }
}
