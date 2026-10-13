using System.ComponentModel.DataAnnotations;
using StudentManagement.Contracts.V1;
using StudentManagement.Web.Resources;

namespace StudentManagement.Web.Models;

/// <summary>Teacher create/edit form. Quick client-side checks only; the server is the source of truth.</summary>
public sealed class TeacherFormModel
{
    [Display(Name = UiText.Code)]
    [Required(ErrorMessage = UiText.Required)]
    [RegularExpression(@"^\s*[A-Za-z0-9]{3,20}\s*$", ErrorMessage = UiText.CodeFormat)]
    public string Code { get; set; } = "";

    [Display(Name = UiText.FullName)]
    [Required(ErrorMessage = UiText.Required)]
    [StringLength(100, ErrorMessage = UiText.MaxLength)]
    public string FullName { get; set; } = "";

    [Display(Name = UiText.DateOfBirth)]
    [Required(ErrorMessage = UiText.RequiredSelect)]
    public DateTime? DateOfBirth { get; set; }

    public void Load(Teacher teacher)
    {
        Code = teacher.Code;
        FullName = teacher.FullName;
        DateOfBirth = teacher.DateOfBirth.ToDateTime();
    }

    public CreateTeacherRequest ToCreateRequest() =>
        new() { Code = Code, FullName = FullName, DateOfBirth = DateOfBirth!.Value.ToProto() };

    public UpdateTeacherRequest ToUpdateRequest(int id, int version) =>
        new() { Id = id, Version = version, Code = Code, FullName = FullName, DateOfBirth = DateOfBirth!.Value.ToProto() };
}
