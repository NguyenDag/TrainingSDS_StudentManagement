using FluentValidation;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Tests.Fakes;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Tests.Services;

public class ClassroomAppServiceTests
{
    private readonly TestDb _db = new();
    private readonly ClassroomAppService _service;
    private readonly Teacher _teacher;

    public ClassroomAppServiceTests()
    {
        _service = _db.ClassroomService();
        _teacher = _db.AddTeacher();
    }

    private ClassroomInput Input(string code = "LH100", string name = "Lớp 11B2", int? teacherId = null) =>
        new(code, name, "Ngữ văn", teacherId ?? _teacher.Id);

    [Fact]
    public async Task List_filters_by_teacher_and_returns_student_counts()
    {
        var other = _db.AddTeacher("GV002");
        var full = _db.AddClassroom(_teacher, "LH001");
        _db.AddClassroom(_teacher, "LH002");
        _db.AddClassroom(other, "LH003");
        _db.AddStudent(full, "SV001");
        _db.AddStudent(full, "SV002");

        var page = await _service.ListAsync(new ClassroomQuery { TeacherId = _teacher.Id });

        page.TotalCount.ShouldBe(2);
        page.Items.Select(c => (c.Code, c.StudentCount)).ShouldBe(new[] { ("LH001", 2), ("LH002", 0) });
        page.Items[0].Teacher.Code.ShouldBe("GV001");
    }

    [Fact]
    public async Task Get_returns_classroom_with_teacher_and_student_count()
    {
        var classroom = _db.AddClassroom(_teacher);
        _db.AddStudent(classroom);

        var dto = await _service.GetAsync(classroom.Id);

        dto.Teacher.ShouldBe(new(_teacher.Id, "GV001", "Trần Thị Bình"));
        dto.StudentCount.ShouldBe(1);
    }

    [Fact]
    public async Task Get_unknown_id_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.GetAsync(99)))
            .ErrorCode.ShouldBe(ErrorCodes.ClassroomNotFound);
    }

    [Fact]
    public async Task Create_stores_classroom_and_commits()
    {
        var dto = await _service.CreateAsync(Input(code: "lh100"));

        dto.Code.ShouldBe("LH100");
        dto.Teacher.Id.ShouldBe(_teacher.Id);
        dto.StudentCount.ShouldBe(0);
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Create_with_invalid_input_throws_validation()
    {
        await Should.ThrowAsync<ValidationException>(() => _service.CreateAsync(Input(name: "")));
        _db.Classrooms.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_with_taken_code_throws_conflict()
    {
        _db.AddClassroom(_teacher, "LH001");

        (await Should.ThrowAsync<ConflictException>(() => _service.CreateAsync(Input(code: "LH001"))))
            .ErrorCode.ShouldBe(ErrorCodes.ClassroomCodeDuplicated);
    }

    [Fact]
    public async Task Create_with_unknown_teacher_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.CreateAsync(Input(teacherId: 99))))
            .ErrorCode.ShouldBe(ErrorCodes.TeacherNotFound);
        _db.UnitOfWork.Commits.ShouldBe(0);
    }

    [Fact]
    public async Task Update_reassigns_teacher_and_bumps_version()
    {
        var classroom = _db.AddClassroom(_teacher, "LH001");
        var other = _db.AddTeacher("GV002");
        _db.AddStudent(classroom);

        var dto = await _service.UpdateAsync(classroom.Id, 1, Input(code: "LH001", teacherId: other.Id));

        dto.Teacher.Code.ShouldBe("GV002");
        dto.StudentCount.ShouldBe(1);
        dto.Version.ShouldBe(2);
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Update_with_invalid_input_throws_validation()
    {
        var classroom = _db.AddClassroom(_teacher);
        await Should.ThrowAsync<ValidationException>(() => _service.UpdateAsync(classroom.Id, 1, Input(teacherId: 0)));
    }

    [Fact]
    public async Task Update_unknown_id_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.UpdateAsync(99, 1, Input())))
            .ErrorCode.ShouldBe(ErrorCodes.ClassroomNotFound);
    }

    [Fact]
    public async Task Update_with_stale_version_throws_concurrency()
    {
        var classroom = _db.AddClassroom(_teacher);
        await Should.ThrowAsync<ConcurrencyException>(() => _service.UpdateAsync(classroom.Id, 5, Input()));
    }

    [Fact]
    public async Task Update_to_code_of_another_classroom_throws_conflict()
    {
        _db.AddClassroom(_teacher, "LH001");
        var classroom = _db.AddClassroom(_teacher, "LH002");

        (await Should.ThrowAsync<ConflictException>(() => _service.UpdateAsync(classroom.Id, 1, Input(code: "LH001"))))
            .ErrorCode.ShouldBe(ErrorCodes.ClassroomCodeDuplicated);
    }

    [Fact]
    public async Task Update_with_unknown_teacher_throws_not_found()
    {
        var classroom = _db.AddClassroom(_teacher);

        (await Should.ThrowAsync<NotFoundException>(() => _service.UpdateAsync(classroom.Id, 1, Input(teacherId: 99))))
            .ErrorCode.ShouldBe(ErrorCodes.TeacherNotFound);
        classroom.Teacher.ShouldBe(_teacher);
    }

    [Fact]
    public async Task Delete_removes_empty_classroom()
    {
        var classroom = _db.AddClassroom(_teacher);

        await _service.DeleteAsync(classroom.Id);

        (await _db.Classrooms.GetAsync(classroom.Id)).ShouldBeNull();
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Delete_unknown_id_throws_not_found()
    {
        await Should.ThrowAsync<NotFoundException>(() => _service.DeleteAsync(99));
    }

    [Fact]
    public async Task Delete_classroom_with_students_throws_in_use()
    {
        var classroom = _db.AddClassroom(_teacher);
        _db.AddStudent(classroom);

        (await Should.ThrowAsync<InUseException>(() => _service.DeleteAsync(classroom.Id)))
            .ErrorCode.ShouldBe(ErrorCodes.ClassroomHasStudents);
        (await _db.Classrooms.GetAsync(classroom.Id)).ShouldNotBeNull();
    }
}
