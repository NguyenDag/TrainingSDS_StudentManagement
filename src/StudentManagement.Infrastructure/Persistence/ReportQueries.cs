using NHibernate;
using NHibernate.Linq;
using StudentManagement.Application.Reports;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Infrastructure.Persistence;

/// <summary>
/// Dashboard aggregates, all computed by PostgreSQL with GROUP BY. Each query is a future,
/// so the whole dashboard costs a single database round trip.
/// </summary>
internal sealed class ReportQueries(ISession session) : IReportQueries
{
    public async Task<ReportData> GetAsync(CancellationToken cancellationToken = default)
    {
        var totalStudents = session.Query<Student>().ToFutureValue(q => q.Count());
        var totalClassrooms = session.Query<Classroom>().ToFutureValue(q => q.Count());
        var totalTeachers = session.Query<Teacher>().ToFutureValue(q => q.Count());

        // HQL entity join ("join X with ..."): the model has no Classroom → Students collection,
        // and a left join keeps classrooms that have no students yet.
        var byClassroom = session.CreateQuery(
                "select c.Name, count(s.Id) from Classroom c left join Student s with s.Classroom = c " +
                "group by c.Id, c.Code, c.Name order by c.Code")
            .Future<object[]>();

        var bySubject = session.Query<Student>()
            .GroupBy(s => s.Classroom.Subject)
            .Select(g => new { Subject = g.Key, Count = g.Count() })
            .ToFuture();

        var byTeacher = session.CreateQuery(
                "select t.FullName, count(c.Id) from Teacher t left join Classroom c with c.Teacher = t " +
                "group by t.Id, t.GivenName, t.FullName order by t.GivenName, t.FullName")
            .Future<object[]>();

        // Native SQL for PostgreSQL's age(), which counts whole years correctly across leap days.
        var byAge = session.CreateSQLQuery(
                "select cast(date_part('year', age(current_date, date_of_birth)) as integer) as age, " +
                "cast(count(*) as integer) as students from students group by 1")
            .AddScalar("age", NHibernateUtil.Int32)
            .AddScalar("students", NHibernateUtil.Int32)
            .Future<object[]>();

        // The first await sends every pending future in one batch.
        var students = await totalStudents.GetValueAsync(cancellationToken);
        return new ReportData(
            students,
            await totalClassrooms.GetValueAsync(cancellationToken),
            await totalTeachers.GetValueAsync(cancellationToken),
            ToCounts(await byClassroom.GetEnumerableAsync(cancellationToken)),
            (await bySubject.GetEnumerableAsync(cancellationToken))
                .OrderByDescending(s => s.Count).ThenBy(s => s.Subject)
                .Select(s => new CountByLabel(s.Subject, s.Count))
                .ToList(),
            (await byAge.GetEnumerableAsync(cancellationToken)).ToDictionary(r => (int)r[0], r => (int)r[1]),
            ToCounts(await byTeacher.GetEnumerableAsync(cancellationToken)));
    }

    // HQL count() comes back as long.
    private static List<CountByLabel> ToCounts(IEnumerable<object[]> rows) =>
        rows.Select(r => new CountByLabel((string)r[0], Convert.ToInt32(r[1]))).ToList();
}
