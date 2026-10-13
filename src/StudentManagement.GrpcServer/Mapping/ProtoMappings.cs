using FluentValidation;
using FluentValidation.Results;
using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Common;
using StudentManagement.Application.Reports;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using Proto = StudentManagement.Contracts.V1;

namespace StudentManagement.GrpcServer.Mapping;

/// <summary>
/// proto ↔ Application DTO, all in one file so a new field is mapped in one place.
/// Proto3 has no null: unset optional fields are checked with Has*, and 0 enums mean "default".
/// </summary>
internal static class ProtoMappings
{
    // ---- Shared ----

    public static Proto.Date ToProto(this DateOnly date) => new() { Year = date.Year, Month = date.Month, Day = date.Day };

    /// <param name="field">Input property the date belongs to, so the error is shown on that form field.</param>
    public static DateOnly ToDateOnly(this Proto.Date? date, string field)
    {
        if (date is { Year: >= 1 and <= 9999, Month: >= 1 and <= 12, Day: >= 1 }
            && date.Day <= DateTime.DaysInMonth(date.Year, date.Month))
            return new DateOnly(date.Year, date.Month, date.Day);
        throw new ValidationException([new ValidationFailure(field, Messages.DateInvalid)]);
    }

    private static SortDirection ToApplication(this Proto.SortDirection direction) =>
        direction == Proto.SortDirection.Desc ? SortDirection.Descending : SortDirection.Ascending;

    public static Proto.PageInfo ToPageInfo<T>(this PagedResult<T> page) =>
        new() { Page = page.Page, PageSize = page.PageSize, TotalCount = page.TotalCount };

    // ---- Teachers ----

    public static TeacherQuery ToQuery(this Proto.ListTeachersRequest request) => new()
    {
        Page = request.Page,
        PageSize = request.PageSize,
        Direction = request.Direction.ToApplication(),
        SortBy = request.SortBy switch
        {
            Proto.TeacherSortField.Code => TeacherSortField.Code,
            Proto.TeacherSortField.DateOfBirth => TeacherSortField.DateOfBirth,
            _ => TeacherSortField.Name,
        },
        Keyword = request.HasKeyword ? request.Keyword : null,
    };

    public static TeacherInput ToInput(this Proto.CreateTeacherRequest request) =>
        new(request.Code, request.FullName, request.DateOfBirth.ToDateOnly(nameof(TeacherInput.DateOfBirth)));

    public static TeacherInput ToInput(this Proto.UpdateTeacherRequest request) =>
        new(request.Code, request.FullName, request.DateOfBirth.ToDateOnly(nameof(TeacherInput.DateOfBirth)));

    public static Proto.TeacherSummary ToProto(this TeacherSummaryDto teacher) =>
        new() { Id = teacher.Id, Code = teacher.Code, FullName = teacher.FullName };

    public static Proto.Teacher ToProto(this TeacherDto teacher) => new()
    {
        Id = teacher.Id,
        Code = teacher.Code,
        FullName = teacher.FullName,
        DateOfBirth = teacher.DateOfBirth.ToProto(),
        ClassroomCount = teacher.ClassroomCount,
        Version = teacher.Version,
    };

    // ---- Classrooms ----

    public static ClassroomQuery ToQuery(this Proto.ListClassroomsRequest request) => new()
    {
        Page = request.Page,
        PageSize = request.PageSize,
        Direction = request.Direction.ToApplication(),
        SortBy = request.SortBy switch
        {
            Proto.ClassroomSortField.Name => ClassroomSortField.Name,
            Proto.ClassroomSortField.Subject => ClassroomSortField.Subject,
            _ => ClassroomSortField.Code,
        },
        Keyword = request.HasKeyword ? request.Keyword : null,
        TeacherId = request.HasTeacherId ? request.TeacherId : null,
    };

    public static ClassroomInput ToInput(this Proto.CreateClassroomRequest request) =>
        new(request.Code, request.Name, request.Subject, request.TeacherId);

    public static ClassroomInput ToInput(this Proto.UpdateClassroomRequest request) =>
        new(request.Code, request.Name, request.Subject, request.TeacherId);

    public static Proto.ClassroomSummary ToProto(this ClassroomSummaryDto classroom) => new()
    {
        Id = classroom.Id,
        Code = classroom.Code,
        Name = classroom.Name,
        Subject = classroom.Subject,
        Teacher = classroom.Teacher.ToProto(),
    };

    public static Proto.Classroom ToProto(this ClassroomDto classroom) => new()
    {
        Id = classroom.Id,
        Code = classroom.Code,
        Name = classroom.Name,
        Subject = classroom.Subject,
        Teacher = classroom.Teacher.ToProto(),
        StudentCount = classroom.StudentCount,
        Version = classroom.Version,
    };

    // ---- Students ----

    public static StudentQuery ToQuery(this Proto.ListStudentsRequest request) => new()
    {
        Page = request.Page,
        PageSize = request.PageSize,
        Direction = request.Direction.ToApplication(),
        SortBy = request.SortBy switch
        {
            Proto.StudentSortField.Code => StudentSortField.Code,
            Proto.StudentSortField.DateOfBirth => StudentSortField.DateOfBirth,
            _ => StudentSortField.Name,
        },
        CodePrefix = request.HasCodePrefix ? request.CodePrefix : null,
        ClassroomId = request.HasClassroomId ? request.ClassroomId : null,
    };

    public static StudentInput ToInput(this Proto.CreateStudentRequest request) =>
        new(request.Code, request.FullName, request.DateOfBirth.ToDateOnly(nameof(StudentInput.DateOfBirth)),
            request.HasAddress ? request.Address : null, request.ClassroomId);

    public static StudentInput ToInput(this Proto.UpdateStudentRequest request) =>
        new(request.Code, request.FullName, request.DateOfBirth.ToDateOnly(nameof(StudentInput.DateOfBirth)),
            request.HasAddress ? request.Address : null, request.ClassroomId);

    public static Proto.Student ToProto(this StudentDto student)
    {
        var message = new Proto.Student
        {
            Id = student.Id,
            Code = student.Code,
            FullName = student.FullName,
            DateOfBirth = student.DateOfBirth.ToProto(),
            Classroom = student.Classroom.ToProto(),
            Version = student.Version,
        };
        if (student.Address is not null) message.Address = student.Address;
        return message;
    }

    // ---- Reports ----

    public static Proto.DashboardResponse ToProto(this DashboardDto dashboard) => new()
    {
        TotalStudents = dashboard.TotalStudents,
        TotalClassrooms = dashboard.TotalClassrooms,
        TotalTeachers = dashboard.TotalTeachers,
        StudentsByClassroom = { dashboard.StudentsByClassroom.Select(ToProto) },
        StudentsBySubject = { dashboard.StudentsBySubject.Select(ToProto) },
        StudentsByAgeGroup = { dashboard.StudentsByAgeGroup.Select(ToProto) },
        ClassroomsByTeacher = { dashboard.ClassroomsByTeacher.Select(ToProto) },
    };

    private static Proto.CountByLabel ToProto(CountByLabel count) => new() { Label = count.Label, Count = count.Count };
}
