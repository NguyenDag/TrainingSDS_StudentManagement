using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Application.Abstractions;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Reports;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using StudentManagement.Application.Tests.Fakes;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_resolves_every_app_service()
    {
        var db = new TestDb();
        var services = new ServiceCollection().AddApplication();
        // Stand-ins for what AddInfrastructure() will register.
        services.AddScoped<IStudentRepository>(_ => db.Students);
        services.AddScoped<IClassroomRepository>(_ => db.Classrooms);
        services.AddScoped<ITeacherRepository>(_ => db.Teachers);
        services.AddScoped<IReadRepository<Classroom>>(_ => db.Classrooms);
        services.AddScoped<IReadRepository<Teacher>>(_ => db.Teachers);
        services.AddScoped<IUnitOfWork>(_ => db.UnitOfWork);
        services.AddScoped<IReportQueries, NoReportQueries>();

        // ValidateScopes catches a singleton capturing a scoped dependency.
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IStudentAppService>().ShouldBeOfType<StudentAppService>();
        scope.ServiceProvider.GetRequiredService<IClassroomAppService>().ShouldBeOfType<ClassroomAppService>();
        scope.ServiceProvider.GetRequiredService<ITeacherAppService>().ShouldBeOfType<TeacherAppService>();
        scope.ServiceProvider.GetRequiredService<IReportAppService>().ShouldBeOfType<ReportAppService>();
    }

    private sealed class NoReportQueries : IReportQueries
    {
        public Task<ReportData> GetAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
