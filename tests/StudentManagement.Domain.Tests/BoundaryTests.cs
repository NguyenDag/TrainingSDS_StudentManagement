using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Exceptions;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;
using static StudentManagement.Domain.Tests.TestData;

namespace StudentManagement.Domain.Tests;

/// <summary>Exactly-at-the-limit cases: the last valid value passes, the first invalid one fails.</summary>
public class BoundaryTests
{
    [Theory]
    [InlineData("abc", "ABC")]                                       // shortest, lower-case accepted
    [InlineData("A1234567890123456789", "A1234567890123456789")]     // 20 characters
    public void Code_at_length_limits_is_accepted(string code, string stored)
    {
        new Teacher(code, "Lê Văn Cường", YearsAgo(30)).Code.ShouldBe(stored);
    }

    [Theory]
    [InlineData("SV 01")] // inner space
    [InlineData("SVĐ01")] // Vietnamese letter
    public void Code_with_other_characters_is_rejected(string code)
    {
        Should.Throw<DomainException>(() => new Teacher(code, "Lê Văn Cường", YearsAgo(30)));
    }

    [Fact]
    public void Full_name_of_100_characters_is_accepted_101_is_not()
    {
        new Teacher("GV001", new string('a', 100), YearsAgo(30)).FullName.Length.ShouldBe(100);
        Should.Throw<DomainException>(() => new Teacher("GV001", new string('a', 101), YearsAgo(30)));
    }

    [Fact]
    public void Address_of_255_characters_is_accepted_256_is_not()
    {
        var student = Student();
        student.ChangeAddress(new string('a', 255));
        student.Address!.Length.ShouldBe(255);
        Should.Throw<DomainException>(() => student.ChangeAddress(new string('a', 256)));
    }

    [Fact]
    public void Classroom_name_of_101_characters_is_rejected()
    {
        Should.Throw<DomainException>(() => new Classroom("LH001", new string('a', 101), "Toán", Teacher()));
    }

    [Fact]
    public void Student_turning_16_today_is_accepted_one_day_younger_is_not()
    {
        new Student("SV042", "Phạm Minh Dũng", YearsAgo(16), null, Classroom()).AgeOn(Today).ShouldBe(16);
        Should.Throw<DomainException>(() => new Student("SV042", "Phạm Minh Dũng", YearsAgo(16).AddDays(1), null, Classroom()));
    }

    [Fact]
    public void Teacher_turning_18_today_is_accepted_one_day_younger_is_not()
    {
        new Teacher("GV042", "Lê Văn Cường", YearsAgo(18)).AgeOn(Today).ShouldBe(18);
        Should.Throw<DomainException>(() => new Teacher("GV042", "Lê Văn Cường", YearsAgo(18).AddDays(1)))
            .Message.ShouldContain("18");
    }

    [Fact]
    public void Earliest_allowed_date_of_birth_is_1900_01_01()
    {
        new Teacher("GV042", "Lê Văn Cường", new DateOnly(1900, 1, 1)).DateOfBirth.ShouldBe(new DateOnly(1900, 1, 1));
    }

    [Fact]
    public void Born_today_is_not_in_the_future_but_is_too_young()
    {
        Should.Throw<DomainException>(() => new Teacher("GV042", "Lê Văn Cường", Today)).Message.ShouldContain("at least");
    }
}
