using FluentValidation;
using StudentManagement.Domain.Common;

namespace StudentManagement.Application.Common;

/// <summary>
/// Rules shared by the input validators. They reuse the domain's own rules (EntityCode,
/// EarliestDateOfBirth, MinimumAgeYears) so the two layers cannot drift apart.
/// </summary>
internal static class ValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidCode<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(Messages.Required)
            .Must(code => EntityCode.IsValid(EntityCode.Normalize(code))).WithMessage(Messages.CodeFormat);

    public static IRuleBuilderOptions<T, string> RequiredText<T>(this IRuleBuilder<T, string> rule, int maxLength) =>
        rule.NotEmpty().WithMessage(Messages.Required)
            .MaximumLength(maxLength).WithMessage(Messages.MaxLength);

    public static IRuleBuilderOptions<T, DateOnly> ValidDateOfBirth<T>(
        this IRuleBuilderInitial<T, DateOnly> rule, int minimumAge, string tooYoungMessage) =>
        rule.Cascade(CascadeMode.Stop)
            .Must(date => date <= Today()).WithMessage(Messages.DateOfBirthInFuture)
            .Must(date => date >= Person.EarliestDateOfBirth).WithMessage(Messages.DateOfBirthTooEarly)
            .Must(date => date <= Today().AddYears(-minimumAge))
            .WithMessage(tooYoungMessage.Replace("{MinimumAge}", minimumAge.ToString()));

    public static IRuleBuilderOptions<T, int> ValidReference<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThan(0).WithMessage(Messages.ReferenceRequired);

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);
}
