using System.Globalization;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StudentManagement.Contracts.V1;
using StudentManagement.Web;

var vietnamese = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = vietnamese;
CultureInfo.DefaultThreadCurrentUICulture = vietnamese;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAntDesign();

// One channel for the app's lifetime. Empty GrpcBaseUrl = same origin (nginx proxies gRPC-Web in Docker).
builder.Services.AddSingleton(_ =>
{
    var configured = builder.Configuration["GrpcBaseUrl"];
    var baseUrl = string.IsNullOrWhiteSpace(configured) ? builder.HostEnvironment.BaseAddress : configured;
    return GrpcChannel.ForAddress(baseUrl, new GrpcChannelOptions
    {
        // Browsers cannot speak native gRPC (no HTTP/2 trailers), hence gRPC-Web over fetch.
        HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()),
    });
});
builder.Services.AddSingleton(sp => new StudentService.StudentServiceClient(sp.GetRequiredService<GrpcChannel>()));
builder.Services.AddSingleton(sp => new ClassroomService.ClassroomServiceClient(sp.GetRequiredService<GrpcChannel>()));
builder.Services.AddSingleton(sp => new TeacherService.TeacherServiceClient(sp.GetRequiredService<GrpcChannel>()));

await builder.Build().RunAsync();
