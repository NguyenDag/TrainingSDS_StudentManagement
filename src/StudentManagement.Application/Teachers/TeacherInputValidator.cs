using FluentValidation;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Common;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Teachers;

public sealed class TeacherInputValidator : AbstractValidator<TeacherInput>
{
    public TeacherInputValidator()
    {
        RuleFor(x => x.Code).ValidCode().WithName(Messages.Code);
        RuleFor(x => x.FullName).RequiredText(Person.FullNameMaxLength).WithName(Messages.FullName);
        RuleFor(x => x.DateOfBirth)
            .ValidDateOfBirth(Teacher.MinimumAgeYears, Messages.TeacherTooYoung)
            .WithName(Messages.DateOfBirth);
    }
}
