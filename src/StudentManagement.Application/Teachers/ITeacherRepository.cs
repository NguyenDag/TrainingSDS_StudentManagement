using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Teachers;

public interface ITeacherRepository : IRepository<Teacher>
{
    /// <param name="code">Normalized code (see <c>EntityCode.Normalize</c>).</param>
    /// <param name="excludeId">The teacher being updated, so it does not conflict with itself.</param>
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<PagedResult<Teacher>> SearchAsync(TeacherQuery query, CancellationToken cancellationToken = default);

    /// <summary>Number of classrooms per teacher. Teachers without classrooms are absent from the result.</summary>
    Task<IReadOnlyDictionary<int, int>> CountClassroomsAsync(
        IReadOnlyCollection<int> teacherIds, CancellationToken cancellationToken = default);
}
