using StudentManagement.Domain.Classrooms;
using StudentManagement.Domain.Students;
using StudentManagement.Domain.Teachers;

namespace StudentManagement.Domain.Tests;

internal static class TestData
{
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public static DateOnly YearsAgo(int years) => Today.AddYears(-years);

    public static Teacher Teacher() => new("GV001", "Trần Thị Bình", YearsAgo(40));

    public static Classroom Classroom() => new("LH001", "Lớp 10A1", "Toán", Teacher());

    public static Student Student() => new("SV001", "Nguyễn Văn An", YearsAgo(18), "Hà Nội", Classroom());
}
