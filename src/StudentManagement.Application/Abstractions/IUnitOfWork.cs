namespace StudentManagement.Application.Abstractions;

/// <summary>
/// Transaction boundary of one use case. Nothing done through repositories is persisted
/// until <see cref="CommitAsync"/>; if it is never called, all changes are discarded.
/// </summary>
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
