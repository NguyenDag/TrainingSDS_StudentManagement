using FluentValidation;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Students;
using StudentManagement.Application.Tests.Fakes;
using StudentManagement.Domain.Classrooms;
using static StudentManagement.Application.Tests.Fakes.TestDb;

namespace StudentManagement.Application.Tests.Services;

public class StudentAppServiceTests
{
    private readonly TestDb _db = new();
    private readonly StudentAppService _service;
    private readonly Classroom _classroom;

    public StudentAppServiceTests()
    {
        _service = _db.StudentService();
        _classroom = _db.AddClassroom(_db.AddTeacher());
    }

    private StudentInput Input(string code = "SV100", string fullName = "Phạm Minh Dũng", int age = 18,
        string? address = "Huế", int? classroomId = null) =>
        new(code, fullName, YearsAgo(age), address, classroomId ?? _classroom.Id);

    [Fact]
    public async Task List_normalizes_code_prefix_and_pages()
    {
        foreach (var code in new[] { "SV001", "SV002", "SV003", "SV010" })
            _db.AddStudent(_classroom, code);

        var page = await _service.ListAsync(new StudentQuery { CodePrefix = " sv00", PageSize = 2, Page = 2 });

        page.TotalCount.ShouldBe(3);
        page.Items.Select(s => s.Code).ShouldBe(new[] { "SV003" });
    }

    [Fact]
    public async Task List_filters_by_classroom_and_maps_nested_classroom_and_teacher()
    {
        var other = _db.AddClassroom(_classroom.Teacher, "LH002");
        _db.AddStudent(_classroom, "SV001");
        _db.AddStudent(other, "SV002");

        var page = await _service.ListAsync(new StudentQuery { ClassroomId = other.Id });

        var student = page.Items.ShouldHaveSingleItem();
        student.Code.ShouldBe("SV002");
        student.Classroom.Code.ShouldBe("LH002");
        student.Classroom.Teacher.Code.ShouldBe("GV001");
    }

    [Fact]
    public async Task Get_returns_student()
    {
        var student = _db.AddStudent(_classroom);

        var dto = await _service.GetAsync(student.Id);

        dto.ShouldBe(new StudentDto(student.Id, "SV001", "Nguyễn Văn An", student.DateOfBirth, "Hà Nội",
            new(_classroom.Id, "LH001", "Lớp 10A1", "Toán", new(_classroom.Teacher.Id, "GV001", "Trần Thị Bình")), 1));
    }

    [Fact]
    public async Task Get_unknown_id_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.GetAsync(99)))
            .ErrorCode.ShouldBe(ErrorCodes.StudentNotFound);
    }

    [Fact]
    public async Task Get_by_code_is_exact_and_case_insensitive()
    {
        _db.AddStudent(_classroom, "SV001");
        _db.AddStudent(_classroom, "SV0011");

        (await _service.GetByCodeAsync(" sv001 ")).Code.ShouldBe("SV001");
    }

    [Fact]
    public async Task Get_by_unknown_code_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.GetByCodeAsync("SV999")))
            .ErrorCode.ShouldBe(ErrorCodes.StudentNotFound);
    }

    [Fact]
    public async Task Create_stores_student_and_commits()
    {
        var dto = await _service.CreateAsync(Input(code: "sv100", address: "  "));

        dto.Code.ShouldBe("SV100");
        dto.Address.ShouldBeNull();
        dto.Classroom.Id.ShouldBe(_classroom.Id);
        dto.Version.ShouldBe(1);
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Create_with_invalid_input_throws_validation()
    {
        await Should.ThrowAsync<ValidationException>(() => _service.CreateAsync(Input(age: 15)));
        _db.Students.Items.ShouldBeEmpty();
        _db.UnitOfWork.Commits.ShouldBe(0);
    }

    [Fact]
    public async Task Create_with_taken_code_throws_conflict()
    {
        _db.AddStudent(_classroom, "SV001");

        (await Should.ThrowAsync<ConflictException>(() => _service.CreateAsync(Input(code: "sv001"))))
            .ErrorCode.ShouldBe(ErrorCodes.StudentCodeDuplicated);
    }

    [Fact]
    public async Task Create_with_unknown_classroom_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.CreateAsync(Input(classroomId: 99))))
            .ErrorCode.ShouldBe(ErrorCodes.ClassroomNotFound);
        _db.Students.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Update_applies_every_field_and_bumps_version()
    {
        var student = _db.AddStudent(_classroom, "SV001");
        var other = _db.AddClassroom(_classroom.Teacher, "LH002");

        var dto = await _service.UpdateAsync(student.Id, 1,
            new StudentInput("SV001", "Nguyễn Văn Ánh", YearsAgo(20), "Đà Lạt", other.Id));

        dto.FullName.ShouldBe("Nguyễn Văn Ánh");
        dto.DateOfBirth.ShouldBe(YearsAgo(20));
        dto.Address.ShouldBe("Đà Lạt");
        dto.Classroom.Code.ShouldBe("LH002");
        dto.Version.ShouldBe(2);
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Update_with_invalid_input_throws_validation()
    {
        var student = _db.AddStudent(_classroom);
        await Should.ThrowAsync<ValidationException>(() => _service.UpdateAsync(student.Id, 1, Input(code: "S-1")));
    }

    [Fact]
    public async Task Update_unknown_id_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.UpdateAsync(99, 1, Input())))
            .ErrorCode.ShouldBe(ErrorCodes.StudentNotFound);
    }

    [Fact]
    public async Task Update_with_stale_version_throws_concurrency()
    {
        var student = _db.AddStudent(_classroom);

        await Should.ThrowAsync<ConcurrencyException>(() => _service.UpdateAsync(student.Id, 2, Input()));
        student.FullName.ShouldBe("Nguyễn Văn An");
    }

    [Fact]
    public async Task Update_to_code_of_another_student_throws_conflict()
    {
        _db.AddStudent(_classroom, "SV001");
        var student = _db.AddStudent(_classroom, "SV002");

        (await Should.ThrowAsync<ConflictException>(() => _service.UpdateAsync(student.Id, 1, Input(code: "SV001"))))
            .ErrorCode.ShouldBe(ErrorCodes.StudentCodeDuplicated);
    }

    [Fact]
    public async Task Update_with_unknown_classroom_throws_not_found()
    {
        var student = _db.AddStudent(_classroom);

        (await Should.ThrowAsync<NotFoundException>(() => _service.UpdateAsync(student.Id, 1, Input(classroomId: 99))))
            .ErrorCode.ShouldBe(ErrorCodes.ClassroomNotFound);
    }

    [Fact]
    public async Task Delete_removes_student_and_commits()
    {
        var student = _db.AddStudent(_classroom);

        await _service.DeleteAsync(student.Id);

        (await _db.Students.GetAsync(student.Id)).ShouldBeNull();
        _db.UnitOfWork.Commits.ShouldBe(1);
    }

    [Fact]
    public async Task Delete_unknown_id_throws_not_found()
    {
        (await Should.ThrowAsync<NotFoundException>(() => _service.DeleteAsync(99)))
            .ErrorCode.ShouldBe(ErrorCodes.StudentNotFound);
    }
}
