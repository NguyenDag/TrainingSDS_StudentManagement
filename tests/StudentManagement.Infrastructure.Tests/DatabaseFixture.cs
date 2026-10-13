using DotNet.Testcontainers.Configurations;
using NHibernate;
using Npgsql;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;
using StudentManagement.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using NHConfiguration = NHibernate.Cfg.Configuration;

namespace StudentManagement.Infrastructure.Tests;

/// <summary>
/// One real PostgreSQL 16 container for the whole test run, initialised by the same db/init
/// scripts as docker compose. NHibernate connects as sm_app, like the application does.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string AppPassword = "sm_app_test";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("studentmanagement")
        .WithEnvironment("APP_DB_PASSWORD", AppPassword)
        .WithBindMount(FindRepositoryPath("db", "init"), "/docker-entrypoint-initdb.d", AccessMode.ReadOnly)
        .Build();

    public NHConfiguration Configuration { get; private set; } = null!;
    public ISessionFactory SessionFactory { get; private set; } = null!;
    public NHibernateOptions Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var appConnection = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Username = "sm_app",
            Password = AppPassword,
        };
        Options = new NHibernateOptions(appConnection.ConnectionString);
        Configuration = SessionFactoryBuilder.BuildConfiguration(Options);
        SessionFactory = Configuration.BuildSessionFactory();
    }

    public async Task DisposeAsync()
    {
        SessionFactory?.Dispose();
        await _container.DisposeAsync();
    }

    public ISession OpenSession() => SessionFactory.OpenAppSession();

    /// <summary>Simulates a database outage.</summary>
    public Task StopAsync() => _container.StopAsync();

    /// <summary>Empties every table (seed data included) so each test starts from a known state.</summary>
    public Task ResetAsync() =>
        ExecuteAsSuperuserAsync("TRUNCATE students, classrooms, teachers RESTART IDENTITY CASCADE");

    /// <summary>Bypasses NHibernate and sm_app's limited rights, e.g. to change data behind a session's back.</summary>
    public async Task ExecuteAsSuperuserAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Saves the entities in their own session and transaction.</summary>
    public async Task SaveAsync(params object[] entities)
    {
        using var session = OpenSession();
        using var transaction = session.BeginTransaction();
        foreach (var entity in entities)
            await session.SaveAsync(entity);
        await transaction.CommitAsync();
    }

    public async Task<(Teacher Teacher, Classroom Classroom)> SaveTeacherWithClassroomAsync()
    {
        var teacher = new Teacher("GV001", "Trần Thị Bình", new DateOnly(1980, 5, 1));
        var classroom = new Classroom("LH001", "Lớp 10A1", "Toán", teacher);
        await SaveAsync(teacher, classroom);
        return (teacher, classroom);
    }

    public static Student NewStudent(Classroom classroom, string code, string fullName = "Nguyễn Văn An") =>
        new(code, fullName, new DateOnly(2006, 3, 15), "Hà Nội", classroom);

    private static string FindRepositoryPath(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "StudentManagement.sln")))
                return Path.Combine([dir.FullName, .. parts]);
        throw new DirectoryNotFoundException("Repository root (StudentManagement.sln) not found.");
    }
}

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}

/// <summary>Base for tests that need the database: wipes it before each test.</summary>
public abstract class DatabaseTest(DatabaseFixture db) : IAsyncLifetime
{
    protected DatabaseFixture Db { get; } = db;

    public Task InitializeAsync() => Db.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
