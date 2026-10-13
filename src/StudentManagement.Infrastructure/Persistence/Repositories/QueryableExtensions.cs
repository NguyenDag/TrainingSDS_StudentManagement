using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Common;

namespace StudentManagement.Infrastructure.Persistence.Repositories;

internal static class QueryableExtensions
{
    public static IQueryable<T> ExceptId<T>(this IQueryable<T> source, int? id) where T : Entity =>
        id is { } excluded ? source.Where(e => e.Id != excluded) : source;

    public static IQueryable<T> SortBy<T>(
        this IQueryable<T> source, IReadOnlyList<Expression<Func<T, object>>> keys, SortDirection direction)
        where T : Entity
    {
        var descending = direction == SortDirection.Descending;
        var ordered = descending ? source.OrderByDescending(keys[0]) : source.OrderBy(keys[0]);
        foreach (var key in keys.Skip(1))
            ordered = descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
        // Id as the last key makes the order total, so LIMIT/OFFSET pages never repeat or skip rows.
        return descending ? ordered.ThenByDescending(e => e.Id) : ordered.ThenBy(e => e.Id);
    }

    /// <summary>Runs the page query and its count in one database round trip (NHibernate futures).</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IFutureEnumerable<T> items, IFutureValue<int> totalCount, PagedQuery query, CancellationToken cancellationToken)
    {
        var page = (await items.GetEnumerableAsync(cancellationToken)).ToList();
        return new PagedResult<T>(page, query.Page, query.PageSize, await totalCount.GetValueAsync(cancellationToken));
    }

    /// <summary>One GROUP BY query: how many rows reference each of <paramref name="ids"/>.</summary>
    public static async Task<IReadOnlyDictionary<int, int>> CountByAsync<T>(
        this IQueryable<T> source, Expression<Func<T, int>> referenceId, IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return new Dictionary<int, int>();

        var rows = await source
            .Where(Contains(ids, referenceId))
            .GroupBy(referenceId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(r => r.Id, r => r.Count);
    }

    // ids.Contains(referenceId(x)), built by hand because the selector is itself an expression.
    private static Expression<Func<T, bool>> Contains<T>(IReadOnlyCollection<int> ids, Expression<Func<T, int>> selector) =>
        Expression.Lambda<Func<T, bool>>(
            Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(int)],
                Expression.Constant(ids), selector.Body),
            selector.Parameters);
}
