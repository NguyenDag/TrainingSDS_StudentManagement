using StudentManagement.Domain.Common;

namespace StudentManagement.Domain.Teachers;

public class Teacher : Person
{
    public const int MinimumAgeYears = 18;

    protected override int MinimumAge => MinimumAgeYears;
    public override string RoleName => "Teacher";

    /// <summary>For NHibernate.</summary>
    protected Teacher() { }

    public Teacher(string code, string fullName, DateOnly dateOfBirth)
        : base(code, fullName, dateOfBirth) { }
}
