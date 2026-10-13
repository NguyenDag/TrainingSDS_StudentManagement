using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Exceptions;
using static StudentManagement.Domain.Tests.TestData;

namespace StudentManagement.Domain.Tests;

public class ClassroomTests
{
    [Fact]
    public void Values_are_normalized()
    {
        var classroom = new Classroom(" lh010 ", " Lớp  11B2 ", " Ngữ văn ", Teacher());

        classroom.Code.ShouldBe("LH010");
        classroom.Name.ShouldBe("Lớp 11B2");
        classroom.Subject.ShouldBe("Ngữ văn");
    }

    [Fact]
    public void Blank_subject_is_rejected()
    {
        Should.Throw<DomainException>(() => new Classroom("LH010", "Lớp 11B2", " ", Teacher()));
    }

    [Fact]
    public void Classroom_must_have_a_teacher()
    {
        Should.Throw<DomainException>(() => Classroom().AssignTeacher(null!));
    }
}
