using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using static StudentManagement.Application.Tests.Fakes.TestDb;

namespace StudentManagement.Application.Tests;

public class ValidatorTests
{
    private static readonly StudentInput ValidStudent = new("SV001", "Nguyễn Văn An", YearsAgo(18), "Hà Nội", 1);

    [Fact]
    public void Valid_student_passes()
    {
        new StudentInputValidator().Validate(ValidStudent).IsValid.ShouldBeTrue();
    }

    public static TheoryData<StudentInput, string, string> InvalidStudents => new()
    {
        { ValidStudent with { Code = "" }, "Code", "Mã số không được để trống." },
        { ValidStudent with { Code = "sv-01" }, "Code", "Mã số gồm 3–20 chữ cái không dấu hoặc chữ số." },
        { ValidStudent with { FullName = "   " }, "FullName", "Họ tên không được để trống." },
        { ValidStudent with { FullName = new string('a', 101) }, "FullName", "Họ tên tối đa 100 ký tự." },
        { ValidStudent with { DateOfBirth = YearsAgo(0).AddDays(1) }, "DateOfBirth", "Ngày sinh không được ở tương lai." },
        { ValidStudent with { DateOfBirth = new DateOnly(1899, 12, 31) }, "DateOfBirth", "Ngày sinh không được trước năm 1900." },
        { ValidStudent with { DateOfBirth = YearsAgo(15) }, "DateOfBirth", "Sinh viên phải từ 16 tuổi trở lên." },
        { ValidStudent with { Address = new string('a', 256) }, "Address", "Địa chỉ tối đa 255 ký tự." },
        { ValidStudent with { ClassroomId = 0 }, "ClassroomId", "Vui lòng chọn Lớp học." },
    };

    [Theory]
    [MemberData(nameof(InvalidStudents))]
    public void Invalid_student_reports_one_vietnamese_error_on_the_field(StudentInput input, string property, string message)
    {
        var error = new StudentInputValidator().Validate(input).Errors.ShouldHaveSingleItem();

        error.PropertyName.ShouldBe(property);
        error.ErrorMessage.ShouldBe(message);
    }

    [Fact]
    public void Teacher_must_be_18_while_student_may_be_17()
    {
        var seventeen = YearsAgo(17);

        new StudentInputValidator().Validate(ValidStudent with { DateOfBirth = seventeen }).IsValid.ShouldBeTrue();
        new TeacherInputValidator().Validate(new TeacherInput("GV001", "Lê Văn Cường", seventeen))
            .Errors.ShouldHaveSingleItem().ErrorMessage.ShouldBe("Giáo viên phải từ 18 tuổi trở lên.");
    }

    [Fact]
    public void Classroom_requires_subject_and_teacher()
    {
        var errors = new ClassroomInputValidator().Validate(new ClassroomInput("LH001", "Lớp 10A1", "", 0)).Errors;

        errors.Select(e => e.ErrorMessage).ShouldBe(new[] { "Môn học không được để trống.", "Vui lòng chọn Giáo viên." });
    }
}
