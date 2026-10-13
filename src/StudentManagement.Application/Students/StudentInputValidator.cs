using FluentValidation;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Students;

namespace StudentManagement.Application.Students;

public sealed class StudentInputValidator : AbstractValidator<StudentInput>
{
    public StudentInputValidator()
    {
        RuleFor(x => x.Code).ValidCode().WithName(Messages.Code);
        RuleFor(x => x.FullName).RequiredText(Person.FullNameMaxLength).WithName(Messages.FullName);
        RuleFor(x => x.DateOfBirth)
            .ValidDateOfBirth(Student.MinimumAgeYears, Messages.StudentTooYoung)
            .WithName(Messages.DateOfBirth);
        RuleFor(x => x.Address)
            .MaximumLength(Student.AddressMaxLength).WithMessage(Messages.MaxLength)
            .WithName(Messages.Address);
        RuleFor(x => x.ClassroomId).ValidReference().WithName(Messages.Classroom);
    }
}
