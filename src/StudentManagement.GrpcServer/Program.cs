using Grpc.AspNetCore.Web;
using StudentManagement.Application;
using StudentManagement.GrpcServer.Interceptors;
using StudentManagement.GrpcServer.Services;
using StudentManagement.Infrastructure;
using StudentManagement.Infrastructure.HealthChecks;

// Composition root: the only place that knows every layer.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddGrpc(options =>
{
    // First added runs outermost, so the log line sees the status the exception interceptor produced.
    options.Interceptors.Add<LoggingInterceptor>();
    options.Interceptors.Add<ExceptionInterceptor>();
});
builder.Services.AddGrpcReflection();
builder.Services.AddHealthChecks().AddDatabase();

// Only needed when the Blazor dev server runs on another origin; in Docker nginx serves both on one origin.
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(corsOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    // Without these the browser hides the gRPC status and error details from the client.
    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding", "Grpc-Status-Details-Bin")));

var app = builder.Build();

await app.Services.ValidateDatabaseSchemaAsync();

app.UseCors();
// Must come before the endpoints; DefaultEnabled turns gRPC-Web on for every service.
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapGrpcService<StudentGrpcService>();
app.MapGrpcService<ClassroomGrpcService>();
app.MapGrpcService<TeacherGrpcService>();
app.MapGrpcService<ReportGrpcService>();
app.MapHealthChecks("/healthz");

if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService(); // lets grpcurl/Postman discover the API without the .proto files

app.Run();

// Lets WebApplicationFactory<Program> in GrpcServer.Tests reach the entry point.
public partial class Program;
