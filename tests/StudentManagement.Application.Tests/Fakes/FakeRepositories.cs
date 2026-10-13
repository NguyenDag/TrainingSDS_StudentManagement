using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Common;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Tests.Fakes;

// Sort order (Vietnamese collation, given name) is the database's job and is tested against
// PostgreSQL in Infrastructure.Tests; the fakes order by code just to be deterministic.

internal sealed class FakeStudentRepository : InMemoryRepository<Student>, IStudentRepository
{
    public Task<Student?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(s => s.Code == code));

    public Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(s => s.Code == code && s.Id != excludeId));

    public Task<PagedResult<Student>> SearchAsync(StudentQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items
            .Where(s => query.CodePrefix is null || s.Code.StartsWith(query.CodePrefix, StringComparison.Ordinal))
            .Where(s => query.ClassroomId is null || s.Classroom.Id == query.ClassroomId)
            .OrderBy(s => s.Code, StringComparer.Ordinal)
            .ToPage(query));
}

internal sealed class FakeClassroomRepository(FakeStudentRepository students)
    : InMemoryRepository<Classroom>, IClassroomRepository
{
    public Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(c => c.Code == code && c.Id != excludeId));

    public Task<PagedResult<Classroom>> SearchAsync(ClassroomQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items
            .Where(c => query.Keyword is null
                        || c.Code.StartsWith(query.Keyword, StringComparison.OrdinalIgnoreCase)
                        || c.Name.Contains(query.Keyword, StringComparison.OrdinalIgnoreCase))
            .Where(c => query.TeacherId is null || c.Teacher.Id == query.TeacherId)
            .OrderBy(c => c.Code, StringComparer.Ordinal)
            .ToPage(query));

    public Task<IReadOnlyDictionary<int, int>> CountStudentsAsync(
        IReadOnlyCollection<int> classroomIds, CancellationToken cancellationToken = default) =>
        Task.FromResult(students.Items.Select(s => s.Classroom.Id).CountWithin(classroomIds));
}

internal sealed class FakeTeacherRepository(FakeClassroomRepository classrooms)
    : InMemoryRepository<Teacher>, ITeacherRepository
{
    public Task<bool> ExistsByCodeAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(t => t.Code == code && t.Id != excludeId));

    public Task<PagedResult<Teacher>> SearchAsync(TeacherQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items
            .Where(t => query.Keyword is null
                        || t.Code.StartsWith(query.Keyword, StringComparison.OrdinalIgnoreCase)
                        || t.FullName.Contains(query.Keyword, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Code, StringComparer.Ordinal)
            .ToPage(query));

    public Task<IReadOnlyDictionary<int, int>> CountClassroomsAsync(
        IReadOnlyCollection<int> teacherIds, CancellationToken cancellationToken = default) =>
        Task.FromResult(classrooms.Items.Select(c => c.Teacher.Id).CountWithin(teacherIds));
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int Commits { get; private set; }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Commits++;
        return Task.CompletedTask;
    }
}

internal static class FakeQueryExtensions
{
    public static PagedResult<T> ToPage<T>(this IEnumerable<T> source, PagedQuery query)
    {
        var all = source.ToList();
        return new PagedResult<T>(all.Skip(query.Skip).Take(query.PageSize).ToList(), query.Page, query.PageSize, all.Count);
    }

    public static IReadOnlyDictionary<int, int> CountWithin(this IEnumerable<int> keys, IReadOnlyCollection<int> wanted) =>
        keys.Where(wanted.Contains).GroupBy(k => k).ToDictionary(g => g.Key, g => g.Count());
}
