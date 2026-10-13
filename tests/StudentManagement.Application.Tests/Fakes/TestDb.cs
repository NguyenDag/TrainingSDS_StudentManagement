using StudentManagement.Application.Classrooms;
using StudentManagement.Application.Students;
using StudentManagement.Application.Teachers;
using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Application.Tests.Fakes;

/// <summary>The fake repositories wired together, plus helpers to seed them and build services.</summary>
internal sealed class TestDb
{
    public TestDb()
    {
        Classrooms = new FakeClassroomRepository(Students);
        Teachers = new FakeTeacherRepository(Classrooms);
    }

    public FakeStudentRepository Students { get; } = new();
    public FakeClassroomRepository Classrooms { get; }
    public FakeTeacherRepository Teachers { get; }
    public FakeUnitOfWork UnitOfWork { get; } = new();

    public static DateOnly YearsAgo(int years) => DateOnly.FromDateTime(DateTime.Today).AddYears(-years);

    public Teacher AddTeacher(string code = "GV001", string fullName = "Trần Thị Bình")
    {
        var teacher = new Teacher(code, fullName, YearsAgo(40));
        Teachers.AddAsync(teacher).Wait();
        return teacher;
    }

    public Classroom AddClassroom(Teacher teacher, string code = "LH001", string name = "Lớp 10A1")
    {
        var classroom = new Classroom(code, name, "Toán", teacher);
        Classrooms.AddAsync(classroom).Wait();
        return classroom;
    }

    public Student AddStudent(Classroom classroom, string code = "SV001", string fullName = "Nguyễn Văn An")
    {
        var student = new Student(code, fullName, YearsAgo(18), "Hà Nội", classroom);
        Students.AddAsync(student).Wait();
        return student;
    }

    public StudentAppService StudentService() =>
        new(Students, Classrooms, UnitOfWork, new StudentInputValidator());

    public ClassroomAppService ClassroomService() =>
        new(Classrooms, Teachers, UnitOfWork, new ClassroomInputValidator());

    public TeacherAppService TeacherService() =>
        new(Teachers, UnitOfWork, new TeacherInputValidator());
}
