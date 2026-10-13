using NHibernate;
using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Tests.Repositories;
using StudentManagement.Domain.Teachers;
using StudentManagement.Infrastructure.Persistence;
using StudentManagement.Infrastructure.Persistence.Repositories;

namespace StudentManagement.Infrastructure.Tests;

/// <summary>The same contract the in-memory fake passes in Application.Tests, now against NHibernate.</summary>
[Collection(DatabaseCollection.Name)]
public class NhRepositoryContractTests(DatabaseFixture db) : RepositoryContractTests, IAsyncLifetime, IDisposable
{
    private readonly List<ISession> _sessions = [];

    protected override (IRepository<Teacher> Repository, IUnitOfWork UnitOfWork) CreateSut()
    {
        var session = db.OpenSession();
        _sessions.Add(session);
        return (new Repository<Teacher>(session), new UnitOfWork(session));
    }

    public Task InitializeAsync() => db.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose() => _sessions.ForEach(s => s.Dispose());
}
