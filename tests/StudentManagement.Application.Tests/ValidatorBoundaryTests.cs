using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using static StudentManagement.Application.Tests.Fakes.TestDb;

namespace StudentManagement.Application.Tests;

/// <summary>Validators at their exact limits: the last valid value passes, the first invalid one fails.</summary>
public class ValidatorBoundaryTests
{
    private static readonly StudentInput Valid = new("SV001", "Nguyễn Văn An", YearsAgo(18), "Hà Nội", 1);
    private static readonly StudentInputValidator Students = new();

    private static string? FirstError(StudentInput input) =>
        Students.Validate(input).Errors.Select(e => e.PropertyName).FirstOrDefault();

    [Theory]
    [InlineData("abc", true)]                     // 3 characters, lower-case allowed (normalized later)
    [InlineData(" sv001 ", true)]                 // surrounding spaces are trimmed
    [InlineData("A1234567890123456789", true)]    // 20 characters
    [InlineData("ab", false)]                     // 2
    [InlineData("A12345678901234567890", false)]  // 21
    [InlineData("SV 01", false)]                  // inner space
    [InlineData("SVĐ01", false)]                  // Vietnamese letter
    public void Code_limits(string code, bool valid)
    {
        FirstError(Valid with { Code = code }).ShouldBe(valid ? null : "Code");
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Full_name_length_limit(int length, bool valid)
    {
        FirstError(Valid with { FullName = new string('a', length) }).ShouldBe(valid ? null : "FullName");
    }

    [Theory]
    [InlineData(255, true)]
    [InlineData(256, false)]
    public void Address_length_limit(int length, bool valid)
    {
        FirstError(Valid with { Address = new string('a', length) }).ShouldBe(valid ? null : "Address");
    }

    [Fact]
    public void Student_minimum_age_is_inclusive()
    {
        FirstError(Valid with { DateOfBirth = YearsAgo(16) }).ShouldBeNull();
        FirstError(Valid with { DateOfBirth = YearsAgo(16).AddDays(1) }).ShouldBe("DateOfBirth");
    }

    [Fact]
    public void Teacher_minimum_age_is_inclusive()
    {
        var validator = new TeacherInputValidator();

        validator.Validate(new TeacherInput("GV001", "Lê Văn Cường", YearsAgo(18))).IsValid.ShouldBeTrue();
        validator.Validate(new TeacherInput("GV001", "Lê Văn Cường", YearsAgo(18).AddDays(1))).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Earliest_date_of_birth_is_inclusive()
    {
        FirstError(Valid with { DateOfBirth = new DateOnly(1900, 1, 1) }).ShouldBeNull();
        FirstError(Valid with { DateOfBirth = new DateOnly(1899, 12, 31) }).ShouldBe("DateOfBirth");
    }

    [Fact]
    public void Classroom_text_limits()
    {
        var validator = new ClassroomInputValidator();
        var valid = new ClassroomInput("LH001", new string('a', 100), new string('b', 100), 1);

        validator.Validate(valid).IsValid.ShouldBeTrue();
        validator.Validate(valid with { Name = new string('a', 101) }).Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Name");
        validator.Validate(valid with { Subject = new string('b', 101) }).Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Subject");
    }
}
