namespace StudentManagement.Infrastructure.Persistence;

/// <param name="ConnectionString">Should use the least-privilege sm_app role, not postgres.</param>
/// <param name="ShowSql">Writes every SQL statement to the console; for development only.</param>
public sealed record NHibernateOptions(string ConnectionString, bool ShowSql = false);
