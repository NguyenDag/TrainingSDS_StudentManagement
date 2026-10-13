using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Exceptions;

namespace StudentManagement.Domain.Students;

public class Student : Person
{
    public const int AddressMaxLength = 255;

    public virtual string? Address { get; protected set; }
    public virtual Classroom Classroom { get; protected set; } = null!;

    public const int MinimumAgeYears = 16;

    protected override int MinimumAge => MinimumAgeYears;
    public override string RoleName => "Student";

    /// <summary>For NHibernate.</summary>
    protected Student() { }

    public Student(string code, string fullName, DateOnly dateOfBirth, string? address, Classroom classroom)
        : base(code, fullName, dateOfBirth)
    {
        ChangeAddress(address);
        MoveTo(classroom);
    }

    public virtual void MoveTo(Classroom classroom) =>
        Classroom = classroom ?? throw new DomainException("Student must belong to a classroom.");

    public virtual void ChangeAddress(string? address) =>
        Address = Guard.OptionalText(address, nameof(Address), AddressMaxLength);

    public override string ToString() => $"{base.ToString()} [{Classroom.Code}]";
}
