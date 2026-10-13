using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using StudentManagement.Infrastructure.Persistence.Mappings;

namespace StudentManagement.Infrastructure.Persistence;

public static class SessionFactoryBuilder
{
    public static Configuration BuildConfiguration(NHibernateOptions options)
    {
        var configuration = new Configuration();
        configuration.DataBaseIntegration(db =>
        {
            db.Dialect<PostgreSQL83Dialect>();
            db.Driver<NpgsqlDriver>();
            db.ConnectionString = options.ConnectionString;
            // NHibernate would otherwise connect while building the factory to read reserved words,
            // so the app could not even start while the database is down.
            db.KeywordsAutoImport = Hbm2DDLKeyWords.None;
            db.LogSqlInConsole = options.ShowSql;
            db.LogFormattedSql = options.ShowSql;
        });
        configuration.AddMapping(CompileMappings());
        return configuration;
    }

    /// <summary>Sessions for application code: SQL is sent only on commit, which keeps writes in one place.</summary>
    internal static ISession OpenAppSession(this ISessionFactory sessionFactory)
    {
        var session = sessionFactory.OpenSession();
        session.FlushMode = FlushMode.Commit;
        return session;
    }

    private static HbmMapping CompileMappings()
    {
        var mapper = new ModelMapper();
        SnakeCaseNamingConvention.ApplyTo(mapper);
        mapper.AddMappings([typeof(TeacherMapping), typeof(ClassroomMapping), typeof(StudentMapping)]);
        return mapper.CompileMappingForAllExplicitlyAddedEntities();
    }
}
