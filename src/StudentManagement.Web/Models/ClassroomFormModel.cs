using System.ComponentModel.DataAnnotations;
using StudentManagement.Contracts.V1;
using StudentManagement.Web.Resources;

namespace StudentManagement.Web.Models;

public sealed class ClassroomFormModel
{
    [Display(Name = UiText.Code)]
    [Required(ErrorMessage = UiText.Required)]
    [RegularExpression(@"^\s*[A-Za-z0-9]{3,20}\s*$", ErrorMessage = UiText.CodeFormat)]
    public string Code { get; set; } = "";

    [Display(Name = UiText.ClassroomName)]
    [Required(ErrorMessage = UiText.Required)]
    [StringLength(100, ErrorMessage = UiText.MaxLength)]
    public string Name { get; set; } = "";

    [Display(Name = UiText.Subject)]
    [Required(ErrorMessage = UiText.Required)]
    [StringLength(100, ErrorMessage = UiText.MaxLength)]
    public string Subject { get; set; } = "";

    [Display(Name = UiText.Teacher)]
    [Required(ErrorMessage = UiText.RequiredSelect)]
    public int? TeacherId { get; set; }

    public void Load(Classroom classroom)
    {
        Code = classroom.Code;
        Name = classroom.Name;
        Subject = classroom.Subject;
        TeacherId = classroom.Teacher.Id;
    }

    public CreateClassroomRequest ToCreateRequest() =>
        new() { Code = Code, Name = Name, Subject = Subject, TeacherId = TeacherId!.Value };

    public UpdateClassroomRequest ToUpdateRequest(int id, int version) =>
        new() { Id = id, Version = version, Code = Code, Name = Name, Subject = Subject, TeacherId = TeacherId!.Value };
}
