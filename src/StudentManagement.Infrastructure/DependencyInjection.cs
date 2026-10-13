using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Teachers;
using StudentManagement.Infrastructure.Persistence;
using StudentManagement.Infrastructure.Persistence.Repositories;
using NHConfiguration = NHibernate.Cfg.Configuration;

namespace StudentManagement.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Reads <c>ConnectionStrings:Default</c> and <c>NHibernate:ShowSql</c>.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
        var showSql = bool.TryParse(configuration["NHibernate:ShowSql"], out var value) && value;
        return services.AddInfrastructure(new NHibernateOptions(connectionString, showSql));
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, NHibernateOptions options)
    {
        var configuration = SessionFactoryBuilder.BuildConfiguration(options);
        services.AddSingleton(configuration);
        // Expensive to build and thread-safe: one per application.
        services.AddSingleton(_ => configuration.BuildSessionFactory());
        // Not thread-safe and holds the first-level cache: one per scope (gRPC call).
        services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenAppSession());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IReadRepository<Classroom>>(sp => sp.GetRequiredService<IClassroomRepository>());
        services.AddScoped<IReadRepository<Teacher>>(sp => sp.GetRequiredService<ITeacherRepository>());
        return services;
    }

    /// <summary>
    /// Compares the mappings with the live database and throws on any missing table or column,
    /// so a mapping/schema mismatch fails at startup instead of on the first request.
    /// </summary>
    public static void ValidateDatabaseSchema(this IServiceProvider services) =>
        new SchemaValidator(services.GetRequiredService<NHConfiguration>()).Validate();
}
