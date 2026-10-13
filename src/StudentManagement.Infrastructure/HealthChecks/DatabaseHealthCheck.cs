using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NHibernate;

namespace StudentManagement.Infrastructure.HealthChecks;

/// <summary>Healthy when a trivial query reaches PostgreSQL with the application's own credentials.</summary>
internal sealed class DatabaseHealthCheck(ISessionFactory sessionFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var session = sessionFactory.OpenStatelessSession();
            await session.CreateSQLQuery("SELECT 1").UniqueResultAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Database is unreachable.", exception);
        }
    }
}

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddDatabase(this IHealthChecksBuilder builder) =>
        builder.AddCheck<DatabaseHealthCheck>("database");
}
