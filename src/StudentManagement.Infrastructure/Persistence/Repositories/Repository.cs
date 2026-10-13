using NHibernate;
using StudentManagement.Application.Abstractions;
using StudentManagement.Domain.Common;

namespace StudentManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Thin wrapper over <see cref="ISession"/>, which already is a repository and unit of work;
/// it exists so Application depends on its own interfaces (DIP), not on NHibernate.
/// </summary>
internal class Repository<T>(ISession session) : IRepository<T> where T : Entity
{
    private static readonly string TouchSql =
        $"UPDATE {SnakeCaseNamingConvention.TableName(typeof(T))} SET updated_at = now() WHERE id = :id";

    protected ISession Session { get; } = session;

    public async Task<T?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await Session.GetAsync<T>(id, cancellationToken);

    // An identity id is only known after INSERT, so NHibernate inserts right here, not at commit.
    public Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        DbExceptionTranslator.RunAsync(() => Session.SaveAsync(entity, cancellationToken));

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Session.UpdateAsync(entity, cancellationToken);
        // updated_at is bookkeeping, not part of the domain model; stamped here rather than by a trigger.
        await Session.CreateSQLQuery(TouchSql).SetInt32("id", entity.Id).ExecuteUpdateAsync(cancellationToken);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default) =>
        // Flushed immediately so a foreign key violation can be reported as "still in use".
        DbExceptionTranslator.RunAsync(async () =>
        {
            await Session.DeleteAsync(entity, cancellationToken);
            await Session.FlushAsync(cancellationToken);
        }, deleting: true);
}
