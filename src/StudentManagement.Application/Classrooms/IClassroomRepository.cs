using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Classrooms;

namespace StudentManagement.Application.Classrooms;

public interface IClassroomRepository : IRepository<Classroom>
{
    /// <param name="code">Normalized code (see <c>EntityCode.Normalize</c>).</param>
    /// <param name="excludeId">The classroom being updated, so it does not conflict with itself.</param>
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<PagedResult<Classroom>> SearchAsync(ClassroomQuery query, CancellationToken cancellationToken = default);

    /// <summary>Class size per classroom. Classrooms without students are absent from the result.</summary>
    Task<IReadOnlyDictionary<int, int>> CountStudentsAsync(
        IReadOnlyCollection<int> classroomIds, CancellationToken cancellationToken = default);
}
