using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Application;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;

namespace StudentManagement.Infrastructure.Tests;

/// <summary>
/// The unchanged Application services running on NHibernate through the real composition
/// (AddApplication + AddInfrastructure): the payoff of depending on interfaces only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class EndToEndTests(DatabaseFixture db) : DatabaseTest(db)
{
    private ServiceProvider BuildProvider() =>
        new ServiceCollection()
            .AddApplication()
            .AddInfrastructure(Db.Options)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

    private static async Task<T> InScopeAsync<T>(ServiceProvider provider, Func<IServiceProvider, Task<T>> action)
    {
        // A fresh scope = a fresh session, like one gRPC call; proves data really reached the database.
        await using var scope = provider.CreateAsyncScope();
        return await action(scope.ServiceProvider);
    }

    [Fact]
    public async Task Create_and_query_through_application_services()
    {
        await using var provider = BuildProvider();

        var teacher = await InScopeAsync(provider, sp => sp.GetRequiredService<ITeacherAppService>()
            .CreateAsync(new TeacherInput("gv001", "Trần Thị Bình", new DateOnly(1980, 5, 1))));
        var classroom = await InScopeAsync(provider, sp => sp.GetRequiredService<IClassroomAppService>()
            .CreateAsync(new ClassroomInput("LH001", "Lớp 10A1", "Toán", teacher.Id)));
        await InScopeAsync(provider, sp => sp.GetRequiredService<IStudentAppService>()
            .CreateAsync(new StudentInput("SV001", "Nguyễn Văn An", new DateOnly(2006, 3, 15), null, classroom.Id)));

        var students = await InScopeAsync(provider, sp => sp.GetRequiredService<IStudentAppService>()
            .ListAsync(new StudentQuery { CodePrefix = "sv" }));
        var teachers = await InScopeAsync(provider, sp => sp.GetRequiredService<ITeacherAppService>()
            .ListAsync(new TeacherQuery()));

        var student = students.Items.ShouldHaveSingleItem();
        student.Classroom.Teacher.Code.ShouldBe("GV001");
        student.Version.ShouldBe(1);
        teachers.Items.ShouldHaveSingleItem().ClassroomCount.ShouldBe(1);
    }

    [Fact]
    public async Task Update_with_stale_version_and_delete_in_use_are_rejected()
    {
        await using var provider = BuildProvider();
        var teacher = await InScopeAsync(provider, sp => sp.GetRequiredService<ITeacherAppService>()
            .CreateAsync(new TeacherInput("GV001", "Trần Thị Bình", new DateOnly(1980, 5, 1))));
        await InScopeAsync(provider, sp => sp.GetRequiredService<IClassroomAppService>()
            .CreateAsync(new ClassroomInput("LH001", "Lớp 10A1", "Toán", teacher.Id)));

        var updated = await InScopeAsync(provider, sp => sp.GetRequiredService<ITeacherAppService>()
            .UpdateAsync(teacher.Id, teacher.Version, new TeacherInput("GV001", "Trần Thị Bích", new DateOnly(1980, 5, 1))));
        updated.Version.ShouldBe(2);

        await Should.ThrowAsync<ConcurrencyException>(() => InScopeAsync(provider, sp => sp.GetRequiredService<ITeacherAppService>()
            .UpdateAsync(teacher.Id, teacher.Version, new TeacherInput("GV001", "X Y", new DateOnly(1980, 5, 1)))));
        await Should.ThrowAsync<InUseException>(() => InScopeAsync(provider, async sp =>
        {
            await sp.GetRequiredService<ITeacherAppService>().DeleteAsync(teacher.Id);
            return 0;
        }));
        await Should.ThrowAsync<ValidationException>(() => InScopeAsync(provider, sp => sp.GetRequiredService<ITeacherAppService>()
            .CreateAsync(new TeacherInput("GV002", "Trẻ Quá", DateOnly.FromDateTime(DateTime.Today).AddYears(-17)))));
    }

    [Fact]
    public void AddInfrastructure_reads_connection_string_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Default"] = Db.Options.ConnectionString })
            .Build();

        using var provider = new ServiceCollection().AddApplication().AddInfrastructure(configuration)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        Should.NotThrow(provider.ValidateDatabaseSchema);
        Should.Throw<InvalidOperationException>(() => new ServiceCollection().AddInfrastructure(new ConfigurationBuilder().Build()));
    }
}
