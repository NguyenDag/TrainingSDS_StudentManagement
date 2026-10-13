using System.Reflection;
using System.Xml.Linq;
using StudentManagement.Application.Students;
using StudentManagement.Domain.Common;
using StudentManagement.Infrastructure.Persistence;

namespace StudentManagement.GrpcServer.Tests;

/// <summary>
/// Guards the dependency rule: references point inwards (Web/GrpcServer → Infrastructure → Application → Domain).
/// Lives here because this is the only test project that sees every server assembly.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly Domain = typeof(Entity).Assembly;
    private static readonly Assembly Application = typeof(StudentAppService).Assembly;
    private static readonly Assembly Infrastructure = typeof(NHibernateOptions).Assembly;

    private static readonly string[] PersistenceLibraries = ["NHibernate", "Npgsql"];
    private static readonly string[] TransportLibraries = ["Grpc", "Google.Protobuf", "Microsoft.AspNetCore"];

    private static string[] ReferencesOf(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name!).ToArray();

    private static bool IsAny(string reference, IEnumerable<string> prefixes) =>
        prefixes.Any(p => reference == p || reference.StartsWith(p + "."));

    [Fact]
    public void Domain_depends_on_the_framework_only()
    {
        // Core-library names (mscorlib/netstandard) can appear once a coverage tool instruments the assembly.
        ReferencesOf(Domain).ShouldAllBe(r => r.StartsWith("System") || r == "mscorlib" || r == "netstandard");
    }

    [Fact]
    public void Application_knows_the_domain_but_no_persistence_or_transport()
    {
        var references = ReferencesOf(Application);

        references.ShouldContain("StudentManagement.Domain");
        references.ShouldNotContain(r => IsAny(r, PersistenceLibraries) || IsAny(r, TransportLibraries));
        references.ShouldNotContain(r => r.StartsWith("StudentManagement.Infrastructure") || r.StartsWith("StudentManagement.GrpcServer"));
    }

    [Fact]
    public void Infrastructure_knows_no_transport()
    {
        var references = ReferencesOf(Infrastructure);

        references.ShouldContain("StudentManagement.Application");
        references.ShouldContain("NHibernate"); // proves the check actually sees third-party references
        references.ShouldNotContain(r => IsAny(r, TransportLibraries) || r.StartsWith("StudentManagement.GrpcServer"));
    }

    [Fact]
    public void Web_shares_nothing_with_the_server_but_the_proto_contract()
    {
        // The Blazor WebAssembly assembly cannot be loaded into this test host, so its project file is checked.
        var project = XDocument.Load(FindRepositoryFile("src", "StudentManagement.Web", "StudentManagement.Web.csproj"));
        var packages = project.Descendants("PackageReference").Select(p => (string)p.Attribute("Include")!).ToArray();

        project.Descendants("ProjectReference").ShouldBeEmpty();
        packages.ShouldNotContain(p => IsAny(p, PersistenceLibraries) || p.StartsWith("FluentValidation"));
        project.Descendants("Protobuf").ShouldAllBe(p => (string)p.Attribute("GrpcServices")! == "Client");
    }

    private static string FindRepositoryFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "StudentManagement.sln")))
                return Path.Combine([dir.FullName, .. parts]);
        throw new DirectoryNotFoundException("Repository root (StudentManagement.sln) not found.");
    }
}
