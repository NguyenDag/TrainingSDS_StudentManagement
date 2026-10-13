using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application;

/// <summary>Entity → DTO mapping, all in one place so a new field is added once.
/// Services never return entities, so callers cannot trigger lazy loading outside the session.</summary>
internal static class DtoMappings
{
    public static TeacherSummaryDto ToSummary(this Teacher teacher) =>
        new(teacher.Id, teacher.Code, teacher.FullName);

    public static TeacherDto ToDto(this Teacher teacher, int classroomCount) =>
        new(teacher.Id, teacher.Code, teacher.FullName, teacher.DateOfBirth, classroomCount, teacher.Version);

    public static ClassroomSummaryDto ToSummary(this Classroom classroom) =>
        new(classroom.Id, classroom.Code, classroom.Name, classroom.Subject, classroom.Teacher.ToSummary());

    public static ClassroomDto ToDto(this Classroom classroom, int studentCount) =>
        new(classroom.Id, classroom.Code, classroom.Name, classroom.Subject, classroom.Teacher.ToSummary(),
            studentCount, classroom.Version);

    public static StudentDto ToDto(this Student student) =>
        new(student.Id, student.Code, student.FullName, student.DateOfBirth, student.Address,
            student.Classroom.ToSummary(), student.Version);
}
