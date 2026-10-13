using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Infrastructure.Persistence.Mappings;

// Column names come from SnakeCaseNamingConvention. Lengths and nullability must match
// db/init/01_schema.sql; SchemaValidator checks that at startup.

internal abstract class EntityMapping<T> : ClassMapping<T> where T : Entity
{
    protected const int CodeLength = 20;

    protected EntityMapping()
    {
        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Identity);
        });
        // Every UPDATE/DELETE adds "WHERE version = ?"; a stale copy then affects no row and fails.
        Version(x => x.Version, m => m.Column("version"));
    }
}

/// <summary>Columns shared by every <see cref="Person"/> table (students, teachers).</summary>
internal abstract class PersonMapping<T> : EntityMapping<T> where T : Person
{
    protected PersonMapping()
    {
        Property(x => x.Code, m => { m.Length(CodeLength); m.NotNullable(true); });
        Property(x => x.FullName, m => { m.Length(Person.FullNameMaxLength); m.NotNullable(true); });
        // Generated column: PostgreSQL computes it, NHibernate never writes it and re-reads it after each write.
        Property(x => x.GivenName, m =>
        {
            m.Length(50);
            m.Generated(PropertyGeneration.Always);
            m.Insert(false);
            m.Update(false);
        });
        Property(x => x.DateOfBirth, m => m.NotNullable(true));
    }
}

internal sealed class TeacherMapping : PersonMapping<Teacher>;

internal sealed class StudentMapping : PersonMapping<Student>
{
    public StudentMapping()
    {
        Property(x => x.Address, m => m.Length(Student.AddressMaxLength));
        ManyToOne(x => x.Classroom, m => m.NotNullable(true));
    }
}

internal sealed class ClassroomMapping : EntityMapping<Classroom>
{
    public ClassroomMapping()
    {
        Property(x => x.Code, m => { m.Length(CodeLength); m.NotNullable(true); });
        Property(x => x.Name, m => { m.Length(Classroom.NameMaxLength); m.NotNullable(true); });
        Property(x => x.Subject, m => { m.Length(Classroom.SubjectMaxLength); m.NotNullable(true); });
        ManyToOne(x => x.Teacher, m => m.NotNullable(true));
    }
}
