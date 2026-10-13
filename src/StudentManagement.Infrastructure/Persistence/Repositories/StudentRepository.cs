using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using StudentManagement.Application.Students;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Students;

namespace StudentManagement.Infrastructure.Persistence.Repositories;

internal sealed class StudentRepository(ISession session) : Repository<Student>(session), IStudentRepository
{
    // Adding a sort option = adding one entry here.
    private static readonly Dictionary<StudentSortField, Expression<Func<Student, object>>[]> SortKeys = new()
    {
        // given_name/full_name use the vi_vn collation in the database: An < Ánh < Bình < ... < Đức.
        [StudentSortField.Name] = [s => s.GivenName, s => s.FullName],
        [StudentSortField.Code] = [s => s.Code],
        [StudentSortField.DateOfBirth] = [s => s.DateOfBirth],
    };

    public async Task<Student?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await Session.Query<Student>()
            .Where(s => s.Code == code)
            .Fetch(s => s.Classroom).ThenFetch(c => c.Teacher)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default) =>
        Session.Query<Student>().ExceptId(excludeId).AnyAsync(s => s.Code == code, cancellationToken);

    public Task<PagedResult<Student>> SearchAsync(StudentQuery query, CancellationToken cancellationToken = default)
    {
        var students = Session.Query<Student>();
        if (!string.IsNullOrEmpty(query.CodePrefix))
        {
            var prefix = query.CodePrefix;
            students = students.Where(s => s.Code.StartsWith(prefix));
        }
        if (query.ClassroomId is { } classroomId)
            students = students.Where(s => s.Classroom.Id == classroomId);

        var total = students.ToFutureValue(q => q.Count());
        // Classroom and teacher come in the same SELECT (join), not one query per row (N+1).
        var page = students
            .SortBy(SortKeys[query.SortBy], query.Direction)
            .Skip(query.Skip).Take(query.PageSize)
            .Fetch(s => s.Classroom).ThenFetch(c => c.Teacher)
            .ToFuture();
        return page.ToPagedResultAsync(total, query, cancellationToken);
    }
}
