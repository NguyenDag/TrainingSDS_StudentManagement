using StudentManagement.Domain.Common;

namespace StudentManagement.Application.Abstractions;

/// <summary>
/// Read-only access by id. Separate from <see cref="IRepository{T}"/> so a service that only
/// needs to look an entity up (e.g. the classroom of a student) cannot modify it.
/// </summary>
public interface IReadRepository<T> where T : Entity
{
    Task<T?> GetAsync(int id, CancellationToken cancellationToken = default);
}
