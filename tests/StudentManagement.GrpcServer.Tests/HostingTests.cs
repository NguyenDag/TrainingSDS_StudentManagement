using Grpc.Core;
using StudentManagement.Contracts.V1;
using StudentManagement.Infrastructure.Tests;

namespace StudentManagement.GrpcServer.Tests;

[Collection(GrpcServerCollection.Name)]
public class HostingTests(GrpcServerFixture fixture) : GrpcTest(fixture)
{
    [Fact]
    public async Task Every_call_is_logged_with_method_status_and_duration()
    {
        await Students.ListStudentsAsync(new ListStudentsRequest());

        App.Logs.Messages.ShouldContain(m =>
            m.StartsWith("gRPC /studentmanagement.v1.StudentService/ListStudents responded OK in ") && m.EndsWith(" ms"));
    }

    [Fact]
    public async Task Services_answer_grpc_web_like_the_browser_client()
    {
        using var channel = App.CreateGrpcWebChannel();
        await CreateTeacherAsync();

        var page = await new TeacherService.TeacherServiceClient(channel).ListTeachersAsync(new ListTeachersRequest());

        page.Items.ShouldHaveSingleItem().Code.ShouldBe("GV001");
    }

    [Fact]
    public async Task Healthz_reports_healthy()
    {
        using var client = App.CreateClient();
        (await client.GetStringAsync("/healthz")).ShouldBe("Healthy");
    }
}

/// <summary>Uses its own container, because it stops the database.</summary>
public class HealthOutageTests : IAsyncLifetime
{
    private readonly DatabaseFixture _database = new();

    public Task InitializeAsync() => _database.InitializeAsync();

    public Task DisposeAsync() => _database.DisposeAsync();

    [Fact]
    public async Task Healthz_turns_unhealthy_when_the_database_stops()
    {
        await using var app = new GrpcApp(_database.Options.ConnectionString);
        using var client = app.CreateClient();
        (await client.GetStringAsync("/healthz")).ShouldBe("Healthy");

        await _database.StopAsync();

        var response = await client.GetAsync("/healthz");
        ((int)response.StatusCode).ShouldBe(503);
        (await response.Content.ReadAsStringAsync()).ShouldBe("Unhealthy");

        // A call during the outage fails as INTERNAL and is logged with its exception…
        using var channel = app.CreateChannel();
        (await Should.ThrowAsync<RpcException>(new StudentService.StudentServiceClient(channel)
            .ListStudentsAsync(new ListStudentsRequest()).ResponseAsync)).StatusCode.ShouldBe(StatusCode.Internal);
        app.Logs.Messages.ShouldContain(m => m.Contains("Unhandled exception"));

        // …yet no log line, exception text included, ever reveals the database password.
        app.Logs.Messages.ShouldNotContain(m => m.Contains(DatabaseFixture.AppPassword) || m.Contains("Password=", StringComparison.OrdinalIgnoreCase));
    }
}
