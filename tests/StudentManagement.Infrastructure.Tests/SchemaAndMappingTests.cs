using NHibernate.Tool.hbm2ddl;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Infrastructure.Tests;

[Collection(DatabaseCollection.Name)]
public class SchemaAndMappingTests(DatabaseFixture db) : DatabaseTest(db)
{
    [Fact]
    public void Mappings_match_the_real_schema()
    {
        Should.NotThrow(() => new SchemaValidator(Db.Configuration).Validate());
    }

    [Fact]
    public async Task Teacher_round_trips()
    {
        var teacher = new Teacher("GV001", "Võ Thị Thu Giang", new DateOnly(1995, 6, 18));
        await Db.SaveAsync(teacher);

        using var session = Db.OpenSession();
        var loaded = await session.GetAsync<Teacher>(teacher.Id);

        loaded.ShouldNotBeSameAs(teacher);
        loaded.Code.ShouldBe("GV001");
        loaded.FullName.ShouldBe("Võ Thị Thu Giang");
        loaded.DateOfBirth.ShouldBe(new DateOnly(1995, 6, 18));
        loaded.GivenName.ShouldBe("Giang"); // computed by PostgreSQL
        loaded.Version.ShouldBe(1);
    }

    [Fact]
    public async Task Classroom_round_trips_with_teacher_reference()
    {
        var (teacher, classroom) = await Db.SaveTeacherWithClassroomAsync();

        using var session = Db.OpenSession();
        var loaded = await session.GetAsync<Classroom>(classroom.Id);

        (loaded.Code, loaded.Name, loaded.Subject).ShouldBe(("LH001", "Lớp 10A1", "Toán"));
        loaded.Teacher.Id.ShouldBe(teacher.Id);
        loaded.Teacher.FullName.ShouldBe("Trần Thị Bình");
    }

    [Fact]
    public async Task Student_round_trips_with_classroom_and_nullable_address()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        var student = new Student("SV001", "Phạm Minh Dũng", new DateOnly(2006, 2, 28), null, classroom);
        await Db.SaveAsync(student);

        using var session = Db.OpenSession();
        var loaded = await session.GetAsync<Student>(student.Id);

        loaded.Code.ShouldBe("SV001");
        loaded.GivenName.ShouldBe("Dũng");
        loaded.DateOfBirth.ShouldBe(new DateOnly(2006, 2, 28));
        loaded.Address.ShouldBeNull();
        loaded.Classroom.Code.ShouldBe("LH001");
        loaded.Classroom.Teacher.Code.ShouldBe("GV001");
    }

    [Fact]
    public async Task Given_name_is_reread_after_update()
    {
        var teacher = new Teacher("GV001", "Lê Văn An", new DateOnly(1980, 1, 1));
        await Db.SaveAsync(teacher);

        using var session = Db.OpenSession();
        using var transaction = session.BeginTransaction();
        var loaded = await session.GetAsync<Teacher>(teacher.Id);
        loaded.Rename("Lê Văn Bình");
        await transaction.CommitAsync();

        loaded.GivenName.ShouldBe("Bình");
        loaded.Version.ShouldBe(2);
    }
}
