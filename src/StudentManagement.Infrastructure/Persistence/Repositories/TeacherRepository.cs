using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using StudentManagement.Application.Common;
using StudentManagement.Application.Teachers;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Infrastructure.Persistence.Repositories;

internal sealed class TeacherRepository(ISession session) : Repository<Teacher>(session), ITeacherRepository
{
    private static readonly Dictionary<TeacherSortField, Expression<Func<Teacher, object>>[]> SortKeys = new()
    {
        [TeacherSortField.Name] = [t => t.GivenName, t => t.FullName],
        [TeacherSortField.Code] = [t => t.Code],
        [TeacherSortField.DateOfBirth] = [t => t.DateOfBirth],
    };

    public Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default) =>
        Session.Query<Teacher>().ExceptId(excludeId).AnyAsync(t => t.Code == code, cancellationToken);

    public Task<PagedResult<Teacher>> SearchAsync(TeacherQuery query, CancellationToken cancellationToken = default)
    {
        var teachers = Session.Query<Teacher>();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var codePrefix = EntityCode.Normalize(query.Keyword);
            var namePart = query.Keyword.Trim().ToLowerInvariant();
            teachers = teachers.Where(t => t.Code.StartsWith(codePrefix) || t.FullName.ToLower().Contains(namePart));
        }

        var total = teachers.ToFutureValue(q => q.Count());
        var page = teachers
            .SortBy(SortKeys[query.SortBy], query.Direction)
            .Skip(query.Skip).Take(query.PageSize)
            .ToFuture();
        return page.ToPagedResultAsync(total, query, cancellationToken);
    }

    public Task<IReadOnlyDictionary<int, int>> CountClassroomsAsync(
        IReadOnlyCollection<int> teacherIds, CancellationToken cancellationToken = default) =>
        Session.Query<Classroom>().CountByAsync(c => c.Teacher.Id, teacherIds, cancellationToken);
}
