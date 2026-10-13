using NHibernate;
using StudentManagement.Application.Abstractions;

namespace StudentManagement.Infrastructure.Persistence;

/// <summary>
/// One database transaction per scope (gRPC call), opened up front so reads and writes of a use case
/// see one consistent snapshot. Disposing without commit rolls everything back.
/// </summary>
internal sealed class UnitOfWork(ISession session) : IUnitOfWork, IDisposable
{
    private ITransaction _transaction = session.BeginTransaction();

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await DbExceptionTranslator.RunAsync(() => _transaction.CommitAsync(cancellationToken));
        _transaction.Dispose();
        // Keep later work in the same scope transactional too.
        _transaction = session.BeginTransaction();
    }

    public void Dispose() => _transaction.Dispose();
}
