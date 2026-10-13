using System.Reflection;
using StudentManagement.Application.Abstractions;
using StudentManagement.Domain.Common;

namespace StudentManagement.Application.Tests.Fakes;

/// <summary>
/// Dictionary-backed <see cref="IRepository{T}"/>. Behaves like the NHibernate one where services
/// can observe it (ids and versions are assigned by the store), verified by the shared contract tests.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : Entity
{
    // Id and Version have protected setters because the store owns them.
    private static readonly PropertyInfo IdProperty = typeof(Entity).GetProperty(nameof(Entity.Id))!;
    private static readonly PropertyInfo VersionProperty = typeof(Entity).GetProperty(nameof(Entity.Version))!;

    private readonly Dictionary<int, T> _items = [];
    private int _lastId;

    public IEnumerable<T> Items => _items.Values;

    public Task<T?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.GetValueOrDefault(id));

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        IdProperty.SetValue(entity, ++_lastId);
        VersionProperty.SetValue(entity, 1);
        _items.Add(entity.Id, entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (!_items.ContainsKey(entity.Id))
            throw new InvalidOperationException($"{typeof(T).Name} {entity.Id} is not stored.");
        VersionProperty.SetValue(entity, entity.Version + 1);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _items.Remove(entity.Id);
        return Task.CompletedTask;
    }
}
