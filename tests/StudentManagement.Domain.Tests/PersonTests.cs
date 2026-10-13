using StudentManagement.Domain.Exceptions;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;
using static StudentManagement.Domain.Tests.TestData;

namespace StudentManagement.Domain.Tests;

public class PersonTests
{
    [Fact]
    public void Code_is_trimmed_and_uppercased()
    {
        new Teacher(" gv042 ", "Lê Văn Cường", YearsAgo(30)).Code.ShouldBe("GV042");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("SV")]                    // too short
    [InlineData("SV-001")]                // punctuation
    [InlineData("đv001")]                 // non-ASCII letter
    [InlineData("SV0123456789012345678")] // 21 chars
    public void Invalid_code_is_rejected(string code)
    {
        Should.Throw<DomainException>(() => new Teacher(code, "Lê Văn Cường", YearsAgo(30)));
    }

    [Fact]
    public void Full_name_is_trimmed_and_inner_whitespace_collapsed()
    {
        new Teacher("GV042", "  Lê   Văn \t Cường ", YearsAgo(30)).FullName.ShouldBe("Lê Văn Cường");
    }

    [Fact]
    public void Blank_full_name_is_rejected()
    {
        Should.Throw<DomainException>(() => new Teacher("GV042", "   ", YearsAgo(30)));
    }

    [Fact]
    public void Future_date_of_birth_is_rejected()
    {
        Should.Throw<DomainException>(() => new Teacher("GV042", "Lê Văn Cường", Today.AddDays(1)));
    }

    [Fact]
    public void Date_of_birth_before_1900_is_rejected()
    {
        Should.Throw<DomainException>(() => new Teacher("GV042", "Lê Văn Cường", new DateOnly(1899, 12, 31)));
    }

    [Fact]
    public void Minimum_age_depends_on_role()
    {
        var seventeen = YearsAgo(17);

        new Student("SV042", "Phạm Minh Dũng", seventeen, null, Classroom()).AgeOn(Today).ShouldBe(17);
        Should.Throw<DomainException>(() => new Teacher("GV042", "Phạm Minh Dũng", seventeen))
            .Message.ShouldContain("18");
    }

    [Fact]
    public void Student_younger_than_16_is_rejected()
    {
        Should.Throw<DomainException>(() => new Student("SV042", "Phạm Minh Dũng", YearsAgo(16).AddDays(1), null, Classroom()));
    }

    [Fact]
    public void Failed_change_keeps_previous_value()
    {
        var student = Student();
        var original = student.DateOfBirth;

        Should.Throw<DomainException>(() => student.ChangeDateOfBirth(YearsAgo(10)));
        student.DateOfBirth.ShouldBe(original);
    }

    [Theory]
    [InlineData("2026-03-14", 25)]
    [InlineData("2026-03-15", 26)]
    public void Age_counts_only_completed_years(string today, int expected)
    {
        var teacher = new Teacher("GV042", "Lê Văn Cường", new DateOnly(2000, 3, 15));
        teacher.AgeOn(DateOnly.Parse(today)).ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Blank_address_is_stored_as_null(string? address)
    {
        var student = Student();
        student.ChangeAddress(address);
        student.Address.ShouldBeNull();
    }

    [Fact]
    public void Student_must_belong_to_a_classroom()
    {
        Should.Throw<DomainException>(() => Student().MoveTo(null!));
    }

    [Fact]
    public void ToString_is_specialized_per_role()
    {
        Student().ToString().ShouldBe("Student SV001 - Nguyễn Văn An [LH001]");
        Teacher().ToString().ShouldBe("Teacher GV001 - Trần Thị Bình");
    }
}
