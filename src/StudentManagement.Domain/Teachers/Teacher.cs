using StudentManagement.Domain.Common;

namespace StudentManagement.Domain.Teachers;

public class Teacher : Person
{
    protected override int MinimumAge => 18;
    public override string RoleName => "Teacher";

    /// <summary>For NHibernate.</summary>
    protected Teacher() { }

    public Teacher(string code, string fullName, DateOnly dateOfBirth)
        : base(code, fullName, dateOfBirth) { }
}
