using StudentManagement.Domain.Common;
using StudentManagement.Domain.Exceptions;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Domain.Classrooms;

public class Classroom : Entity
{
    public const int NameMaxLength = 100;
    public const int SubjectMaxLength = 100;

    public virtual string Code { get; protected set; } = null!;
    public virtual string Name { get; protected set; } = null!;
    public virtual string Subject { get; protected set; } = null!;
    public virtual Teacher Teacher { get; protected set; } = null!;

    /// <summary>For NHibernate.</summary>
    protected Classroom() { }

    public Classroom(string code, string name, string subject, Teacher teacher)
    {
        ChangeCode(code);
        Rename(name);
        ChangeSubject(subject);
        AssignTeacher(teacher);
    }

    public virtual void ChangeCode(string code) => Code = Guard.Code(code);

    public virtual void Rename(string name) =>
        Name = Guard.RequiredText(name, nameof(Name), NameMaxLength);

    public virtual void ChangeSubject(string subject) =>
        Subject = Guard.RequiredText(subject, nameof(Subject), SubjectMaxLength);

    public virtual void AssignTeacher(Teacher teacher) =>
        Teacher = teacher ?? throw new DomainException("Classroom must have a teacher.");

    public override string ToString() => $"{Code} - {Name} ({Subject})";
}
