using System.Text.RegularExpressions;
using NHibernate.Mapping.ByCode;

namespace StudentManagement.Infrastructure.Persistence;

/// <summary>
/// C# PascalCase → PostgreSQL snake_case, applied once to every mapping instead of naming each
/// column by hand: Student → students, FullName → full_name, Classroom (reference) → classroom_id.
/// </summary>
internal static partial class SnakeCaseNamingConvention
{
    public static void ApplyTo(ModelMapper mapper)
    {
        mapper.BeforeMapClass += (_, type, map) => map.Table(TableName(type));
        mapper.BeforeMapProperty += (_, member, map) => map.Column(ToSnakeCase(member.LocalMember.Name));
        mapper.BeforeMapManyToOne += (_, member, map) => map.Column(ToSnakeCase(member.LocalMember.Name) + "_id");
    }

    public static string TableName(Type entityType) => ToSnakeCase(entityType.Name) + "s";

    public static string ToSnakeCase(string name) => UpperCaseInsideWord().Replace(name, "_$1").ToLowerInvariant();

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex UpperCaseInsideWord();
}
