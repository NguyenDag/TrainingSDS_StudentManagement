using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Students;

namespace StudentManagement.Application.Students;

public interface IStudentRepository : IRepository<Student>
{
    /// <param name="code">Normalized code (see <c>EntityCode.Normalize</c>).</param>
    Task<Student?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <param name="code">Normalized code (see <c>EntityCode.Normalize</c>).</param>
    /// <param name="excludeId">The student being updated, so it does not conflict with itself.</param>
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>Loads each student together with its classroom and teacher.</summary>
    /// <remarks><see cref="StudentQuery.CodePrefix"/> is already normalized.</remarks>
    Task<PagedResult<Student>> SearchAsync(StudentQuery query, CancellationToken cancellationToken = default);
}
