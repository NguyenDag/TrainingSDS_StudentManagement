using StudentManagement.Application.Abstractions;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Tests.Repositories;

/// <summary>
/// Behaviour every <see cref="IRepository{T}"/> implementation must share, so services can use any
/// of them interchangeably (Liskov). Infrastructure.Tests runs this same suite against NHibernate.
/// </summary>
public abstract class RepositoryContractTests
{
    protected abstract (IRepository<Teacher> Repository, IUnitOfWork UnitOfWork) CreateSut();

    private static Teacher NewTeacher(string code) => new(code, "Lê Văn Cường", new DateOnly(1980, 1, 1));

    [Fact]
    public async Task Added_entity_gets_an_id_and_can_be_read_back()
    {
        var (repository, unitOfWork) = CreateSut();
        var teacher = NewTeacher("GV100");

        await repository.AddAsync(teacher);
        await unitOfWork.CommitAsync();

        teacher.Id.ShouldBeGreaterThan(0);
        teacher.Version.ShouldBe(1);
        (await repository.GetAsync(teacher.Id)).ShouldBe(teacher);
    }

    [Fact]
    public async Task Each_added_entity_gets_its_own_id()
    {
        var (repository, unitOfWork) = CreateSut();
        var first = NewTeacher("GV101");
        var second = NewTeacher("GV102");

        await repository.AddAsync(first);
        await repository.AddAsync(second);
        await unitOfWork.CommitAsync();

        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public async Task Unknown_id_reads_as_null()
    {
        var (repository, _) = CreateSut();
        (await repository.GetAsync(int.MaxValue)).ShouldBeNull();
    }

    [Fact]
    public async Task Update_persists_changes_and_increments_version()
    {
        var (repository, unitOfWork) = CreateSut();
        var teacher = NewTeacher("GV103");
        await repository.AddAsync(teacher);
        await unitOfWork.CommitAsync();

        teacher.Rename("Lê Văn Đạt");
        await repository.UpdateAsync(teacher);
        await unitOfWork.CommitAsync();

        var reloaded = await repository.GetAsync(teacher.Id);
        reloaded.ShouldNotBeNull().FullName.ShouldBe("Lê Văn Đạt");
        reloaded.Version.ShouldBe(2);
    }

    [Fact]
    public async Task Deleted_entity_can_no_longer_be_read()
    {
        var (repository, unitOfWork) = CreateSut();
        var teacher = NewTeacher("GV104");
        await repository.AddAsync(teacher);
        await unitOfWork.CommitAsync();

        await repository.DeleteAsync(teacher);
        await unitOfWork.CommitAsync();

        (await repository.GetAsync(teacher.Id)).ShouldBeNull();
    }
}
