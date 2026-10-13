using FluentValidation;
using StudentManagement.Application.Common;
using StudentManagement.Domain.Classrooms;

namespace StudentManagement.Application.Classrooms;

public sealed class ClassroomInputValidator : AbstractValidator<ClassroomInput>
{
    public ClassroomInputValidator()
    {
        RuleFor(x => x.Code).ValidCode().WithName(Messages.Code);
        RuleFor(x => x.Name).RequiredText(Classroom.NameMaxLength).WithName(Messages.ClassroomName);
        RuleFor(x => x.Subject).RequiredText(Classroom.SubjectMaxLength).WithName(Messages.Subject);
        RuleFor(x => x.TeacherId).ValidReference().WithName(Messages.Teacher);
    }
}
