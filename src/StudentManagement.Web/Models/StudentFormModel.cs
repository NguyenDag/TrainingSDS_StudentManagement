using System.ComponentModel.DataAnnotations;
using StudentManagement.Contracts.V1;
using StudentManagement.Web.Resources;

namespace StudentManagement.Web.Models;

public sealed class StudentFormModel
{
    [Display(Name = UiText.StudentCode)]
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

    [Display(Name = UiText.Address)]
    [StringLength(255, ErrorMessage = UiText.MaxLength)]
    public string? Address { get; set; }

    [Display(Name = UiText.Classroom)]
    [Required(ErrorMessage = UiText.RequiredSelect)]
    public int? ClassroomId { get; set; }

    public void Load(Student student)
    {
        Code = student.Code;
        FullName = student.FullName;
        DateOfBirth = student.DateOfBirth.ToDateTime();
        Address = student.HasAddress ? student.Address : null;
        ClassroomId = student.Classroom.Id;
    }

    public CreateStudentRequest ToCreateRequest()
    {
        var request = new CreateStudentRequest
        {
            Code = Code, FullName = FullName, DateOfBirth = DateOfBirth!.Value.ToProto(), ClassroomId = ClassroomId!.Value,
        };
        // Unset (not "") so the server stores no address.
        if (!string.IsNullOrWhiteSpace(Address)) request.Address = Address;
        return request;
    }

    public UpdateStudentRequest ToUpdateRequest(int id, int version)
    {
        var request = new UpdateStudentRequest
        {
            Id = id, Version = version, Code = Code, FullName = FullName,
            DateOfBirth = DateOfBirth!.Value.ToProto(), ClassroomId = ClassroomId!.Value,
        };
        if (!string.IsNullOrWhiteSpace(Address)) request.Address = Address;
        return request;
    }
}
