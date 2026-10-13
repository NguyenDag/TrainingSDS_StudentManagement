using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using StudentManagement.Contracts.V1;
using StudentManagement.Infrastructure.Tests;

namespace StudentManagement.GrpcServer.Tests;

/// <summary>The real server (Program.cs) in memory, pointed at a test PostgreSQL database.</summary>
public sealed class GrpcApp(string connectionString) : WebApplicationFactory<Program>
{
    public ListLoggerProvider Logs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
    }

    /// <summary>Native gRPC (HTTP/2), like grpcurl.</summary>
    public GrpcChannel CreateChannel() =>
        GrpcChannel.ForAddress(Server.BaseAddress, new GrpcChannelOptions { HttpHandler = Server.CreateHandler() });

    /// <summary>gRPC-Web, like the Blazor WebAssembly client.</summary>
    public GrpcChannel CreateGrpcWebChannel() =>
        GrpcChannel.ForAddress(Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, Server.CreateHandler()),
        });
}

/// <summary>One database container and one server for the whole collection.</summary>
public sealed class GrpcServerFixture : IAsyncLifetime
{
    public DatabaseFixture Database { get; } = new();
    public GrpcApp App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Database.InitializeAsync();
        App = new GrpcApp(Database.Options.ConnectionString);
        _ = App.Server; // start now, so the first test does not pay for startup
    }

    public async Task DisposeAsync()
    {
        await App.DisposeAsync();
        await Database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class GrpcServerCollection : ICollectionFixture<GrpcServerFixture>
{
    public const string Name = "GrpcServer";
}

/// <summary>Empty database before each test, plus typed clients over one channel.</summary>
public abstract class GrpcTest(GrpcServerFixture fixture) : IAsyncLifetime
{
    private readonly GrpcChannel _channel = fixture.App.CreateChannel();

    protected GrpcApp App => fixture.App;
    protected StudentService.StudentServiceClient Students => new(_channel);
    protected ClassroomService.ClassroomServiceClient Classrooms => new(_channel);
    protected TeacherService.TeacherServiceClient Teachers => new(_channel);

    public Task InitializeAsync() => fixture.Database.ResetAsync();

    public Task DisposeAsync()
    {
        _channel.Dispose();
        return Task.CompletedTask;
    }

    protected static Date Date(int year, int month, int day) => new() { Year = year, Month = month, Day = day };

    protected Task<Teacher> CreateTeacherAsync(string code = "GV001") =>
        Teachers.CreateTeacherAsync(new CreateTeacherRequest { Code = code, FullName = "Trần Thị Bình", DateOfBirth = Date(1980, 5, 1) }).ResponseAsync;

    protected Task<Classroom> CreateClassroomAsync(int teacherId, string code = "LH001") =>
        Classrooms.CreateClassroomAsync(new CreateClassroomRequest { Code = code, Name = "Lớp 10A1", Subject = "Toán", TeacherId = teacherId }).ResponseAsync;

    protected Task<Student> CreateStudentAsync(int classroomId, string code = "SV001") =>
        Students.CreateStudentAsync(new CreateStudentRequest
        {
            Code = code, FullName = "Nguyễn Văn An", DateOfBirth = Date(2006, 3, 15), Address = "Hà Nội", ClassroomId = classroomId,
        }).ResponseAsync;
}

/// <summary>Captures log messages so tests can assert on them.</summary>
public sealed class ListLoggerProvider : ILoggerProvider
{
    public ConcurrentQueue<string> Messages { get; } = new();

    public ILogger CreateLogger(string categoryName) => new ListLogger(Messages);

    public void Dispose() { }

    private sealed class ListLogger(ConcurrentQueue<string> messages) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => messages.Enqueue(formatter(state, exception));
    }
}
