using NHibernate;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Teachers;
using StudentManagement.Infrastructure.Persistence;
using StudentManagement.Infrastructure.Persistence.Repositories;
using static StudentManagement.Infrastructure.Tests.DatabaseFixture;

namespace StudentManagement.Infrastructure.Tests;

[Collection(DatabaseCollection.Name)]
public class ConstraintAndConcurrencyTests(DatabaseFixture db) : DatabaseTest(db)
{
    [Fact]
    public async Task Second_session_saving_a_stale_copy_gets_StaleObjectStateException()
    {
        var teacher = new Teacher("GV001", "Lê Văn Cường", new DateOnly(1980, 1, 1));
        await Db.SaveAsync(teacher);

        using var first = Db.OpenSession();
        using var second = Db.OpenSession();
        using var firstTx = first.BeginTransaction();
        using var secondTx = second.BeginTransaction();
        (await first.GetAsync<Teacher>(teacher.Id)).Rename("Lê Văn Đạt");
        (await second.GetAsync<Teacher>(teacher.Id)).Rename("Lê Văn Hùng");

        await firstTx.CommitAsync();
        await Should.ThrowAsync<StaleObjectStateException>(() => secondTx.CommitAsync());
    }

    [Fact]
    public async Task Unit_of_work_reports_a_stale_update_as_ConcurrencyException()
    {
        var teacher = new Teacher("GV001", "Lê Văn Cường", new DateOnly(1980, 1, 1));
        await Db.SaveAsync(teacher);
        using var session = Db.OpenSession();
        using var unitOfWork = new UnitOfWork(session);
        var repository = new TeacherRepository(session);
        var loaded = (await repository.GetAsync(teacher.Id))!;

        await Db.ExecuteAsSuperuserAsync($"UPDATE teachers SET version = version + 1 WHERE id = {teacher.Id}");
        loaded.Rename("Lê Văn Đạt");
        await repository.UpdateAsync(loaded);

        await Should.ThrowAsync<ConcurrencyException>(() => unitOfWork.CommitAsync());
    }

    [Fact]
    public async Task Duplicate_code_is_reported_as_ConflictException()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        await Db.SaveAsync(NewStudent(classroom, "SV001"));
        using var session = Db.OpenSession();
        using var unitOfWork = new UnitOfWork(session);

        var exception = await Should.ThrowAsync<ConflictException>(() =>
            new StudentRepository(session).AddAsync(NewStudent(session.Load<Classroom>(classroom.Id), "SV001")));

        exception.ErrorCode.ShouldBe(ErrorCodes.StudentCodeDuplicated);
    }

    [Fact]
    public async Task Deleting_a_classroom_with_students_is_reported_as_InUseException()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        await Db.SaveAsync(NewStudent(classroom, "SV001"));
        using var session = Db.OpenSession();
        using var unitOfWork = new UnitOfWork(session);
        var repository = new ClassroomRepository(session);

        var exception = await Should.ThrowAsync<InUseException>(async () =>
            await repository.DeleteAsync((await repository.GetAsync(classroom.Id))!));

        exception.ErrorCode.ShouldBe(ErrorCodes.ClassroomHasStudents);
    }

    [Fact]
    public async Task Deleting_a_teacher_with_classrooms_is_reported_as_InUseException()
    {
        var (teacher, _) = await Db.SaveTeacherWithClassroomAsync();
        using var session = Db.OpenSession();
        using var unitOfWork = new UnitOfWork(session);
        var repository = new TeacherRepository(session);

        var exception = await Should.ThrowAsync<InUseException>(async () =>
            await repository.DeleteAsync((await repository.GetAsync(teacher.Id))!));

        exception.ErrorCode.ShouldBe(ErrorCodes.TeacherHasClassrooms);
    }

    [Fact]
    public async Task Referencing_a_classroom_deleted_meanwhile_is_reported_as_NotFoundException()
    {
        var (_, classroom) = await Db.SaveTeacherWithClassroomAsync();
        using var session = Db.OpenSession();
        using var unitOfWork = new UnitOfWork(session);
        var loaded = (await new ClassroomRepository(session).GetAsync(classroom.Id))!;

        await Db.ExecuteAsSuperuserAsync($"DELETE FROM classrooms WHERE id = {classroom.Id}");

        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            new StudentRepository(session).AddAsync(NewStudent(loaded, "SV001")));
        exception.ErrorCode.ShouldBe(ErrorCodes.ClassroomNotFound);
    }

    [Fact]
    public async Task Nothing_is_persisted_without_commit()
    {
        using (var session = Db.OpenSession())
        using (new UnitOfWork(session))
            await new TeacherRepository(session).AddAsync(new Teacher("GV001", "Lê Văn Cường", new DateOnly(1980, 1, 1)));

        using var check = Db.OpenSession();
        (await new TeacherRepository(check).ExistsByCodeAsync("GV001")).ShouldBeFalse();
    }
}
