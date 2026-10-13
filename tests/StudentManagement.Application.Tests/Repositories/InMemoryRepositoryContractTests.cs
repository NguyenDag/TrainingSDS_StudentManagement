using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Tests.Fakes;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Tests.Repositories;

public class InMemoryRepositoryContractTests : RepositoryContractTests
{
    protected override (IRepository<Teacher> Repository, IUnitOfWork UnitOfWork) CreateSut() =>
        (new InMemoryRepository<Teacher>(), new FakeUnitOfWork());
}
