using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Students;

namespace StudentManagement.Infrastructure.Persistence.Repositories;

internal sealed class ClassroomRepository(ISession session) : Repository<Classroom>(session), IClassroomRepository
{
    private static readonly Dictionary<ClassroomSortField, Expression<Func<Classroom, object>>[]> SortKeys = new()
    {
        [ClassroomSortField.Code] = [c => c.Code],
        [ClassroomSortField.Name] = [c => c.Name],
        [ClassroomSortField.Subject] = [c => c.Subject, c => c.Name],
    };

    public Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default) =>
        Session.Query<Classroom>().ExceptId(excludeId).AnyAsync(c => c.Code == code, cancellationToken);

    public Task<PagedResult<Classroom>> SearchAsync(ClassroomQuery query, CancellationToken cancellationToken = default)
    {
        var classrooms = Session.Query<Classroom>();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var codePrefix = EntityCode.Normalize(query.Keyword);
            var namePart = query.Keyword.Trim().ToLowerInvariant();
            classrooms = classrooms.Where(c => c.Code.StartsWith(codePrefix) || c.Name.ToLower().Contains(namePart));
        }
        if (query.TeacherId is { } teacherId)
            classrooms = classrooms.Where(c => c.Teacher.Id == teacherId);

        var total = classrooms.ToFutureValue(q => q.Count());
        var page = classrooms
            .SortBy(SortKeys[query.SortBy], query.Direction)
            .Skip(query.Skip).Take(query.PageSize)
            .Fetch(c => c.Teacher)
            .ToFuture();
        return page.ToPagedResultAsync(total, query, cancellationToken);
    }

    public Task<IReadOnlyDictionary<int, int>> CountStudentsAsync(
        IReadOnlyCollection<int> classroomIds, CancellationToken cancellationToken = default) =>
        Session.Query<Student>().CountByAsync(s => s.Classroom.Id, classroomIds, cancellationToken);
}
