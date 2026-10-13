using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;

namespace StudentManagement.Application;

public static class DependencyInjection
{
    /// <summary>Registers use cases and validators. Repositories and the unit of work come from Infrastructure.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped: they share the request's unit of work through the repositories.
        services.AddScoped<IStudentAppService, StudentAppService>();
        services.AddScoped<IClassroomAppService, ClassroomAppService>();
        services.AddScoped<ITeacherAppService, TeacherAppService>();

        // Validators hold no state, so one instance can serve every request.
        services.AddValidatorsFromAssemblyContaining<StudentInputValidator>(ServiceLifetime.Singleton);
        return services;
    }
}
