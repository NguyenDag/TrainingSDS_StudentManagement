using StudentManagement.Domain.Common;

namespace StudentManagement.Application.Abstractions;

/// <summary>Collection-like access to one aggregate. Changes are persisted by <see cref="IUnitOfWork.CommitAsync"/>.</summary>
public interface IRepository<T> : IReadRepository<T> where T : Entity
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
