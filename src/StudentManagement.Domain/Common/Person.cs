using StudentManagement.Domain.Exceptions;

namespace StudentManagement.Domain.Common;

public abstract class Person : Entity
{
    public const int FullNameMaxLength = 100;
    public static readonly DateOnly EarliestDateOfBirth = new(1900, 1, 1);

    public virtual string Code { get; protected set; } = null!;
    public virtual string FullName { get; protected set; } = null!;

    /// <summary>Last word of the full name, used for Vietnamese "sort by name".
    /// Computed by the database (generated column) so there is a single source of truth.</summary>
    public virtual string GivenName { get; protected set; } = string.Empty;

    public virtual DateOnly DateOfBirth { get; protected set; }

    /// <summary>Hook for the age rule in <see cref="ChangeDateOfBirth"/>; each role sets its own limit.</summary>
    protected abstract int MinimumAge { get; }

    public abstract string RoleName { get; }

    /// <summary>For NHibernate.</summary>
    protected Person() { }

    protected Person(string code, string fullName, DateOnly dateOfBirth)
    {
        ChangeCode(code);
        Rename(fullName);
        ChangeDateOfBirth(dateOfBirth);
    }

    public virtual int AgeOn(DateOnly today) => AgeBetween(DateOfBirth, today);

    public virtual void ChangeCode(string code) => Code = Guard.Code(code);

    public virtual void Rename(string fullName) =>
        FullName = Guard.RequiredText(fullName, nameof(FullName), FullNameMaxLength);

    public virtual void ChangeDateOfBirth(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (dateOfBirth > today)
            throw new DomainException("Date of birth cannot be in the future.");
        if (dateOfBirth < EarliestDateOfBirth)
            throw new DomainException($"Date of birth cannot be before {EarliestDateOfBirth:yyyy-MM-dd}.");
        if (AgeBetween(dateOfBirth, today) < MinimumAge)
            throw new DomainException($"{RoleName} must be at least {MinimumAge} years old.");
        DateOfBirth = dateOfBirth;
    }

    public override string ToString() => $"{RoleName} {Code} - {FullName}";

    private static int AgeBetween(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        return dateOfBirth.AddYears(age) > today ? age - 1 : age;
    }
}
