using Grpc.Core;
using StudentManagement.Contracts.V1;

namespace StudentManagement.GrpcServer.Tests;

[Collection(GrpcServerCollection.Name)]
public class CrudTests(GrpcServerFixture fixture) : GrpcTest(fixture)
{
    [Fact]
    public async Task Teacher_crud()
    {
        var created = await CreateTeacherAsync(code: "gv001");
        created.Code.ShouldBe("GV001");
        created.Version.ShouldBe(1);

        var fetched = await Teachers.GetTeacherAsync(new GetTeacherRequest { Id = created.Id });
        fetched.ShouldBe(created);

        var updated = await Teachers.UpdateTeacherAsync(new UpdateTeacherRequest
        {
            Id = created.Id, Version = created.Version, Code = "GV001", FullName = "Trần Thị Bích", DateOfBirth = Date(1981, 6, 2),
        });
        updated.FullName.ShouldBe("Trần Thị Bích");
        updated.DateOfBirth.ShouldBe(Date(1981, 6, 2));
        updated.Version.ShouldBe(2);

        var list = await Teachers.ListTeachersAsync(new ListTeachersRequest { Keyword = "bích" });
        list.Items.ShouldHaveSingleItem().Id.ShouldBe(created.Id);
        list.PageInfo.ShouldBe(new PageInfo { Page = 1, PageSize = 20, TotalCount = 1 });

        await Teachers.DeleteTeacherAsync(new DeleteTeacherRequest { Id = created.Id });
        (await Should.ThrowAsync<RpcException>(Teachers.GetTeacherAsync(new GetTeacherRequest { Id = created.Id }).ResponseAsync))
            .StatusCode.ShouldBe(StatusCode.NotFound);
    }

    [Fact]
    public async Task Classroom_crud()
    {
        var teacher = await CreateTeacherAsync();
        var other = await CreateTeacherAsync("GV002");
        var created = await CreateClassroomAsync(teacher.Id);
        created.Teacher.Code.ShouldBe("GV001");
        created.StudentCount.ShouldBe(0);

        var updated = await Classrooms.UpdateClassroomAsync(new UpdateClassroomRequest
        {
            Id = created.Id, Version = created.Version, Code = "LH001", Name = "Lớp 10A9", Subject = "Lý", TeacherId = other.Id,
        });
        updated.Teacher.Code.ShouldBe("GV002");
        updated.Version.ShouldBe(2);

        var list = await Classrooms.ListClassroomsAsync(new ListClassroomsRequest { TeacherId = other.Id });
        list.Items.ShouldHaveSingleItem().Name.ShouldBe("Lớp 10A9");
        (await Teachers.GetTeacherAsync(new GetTeacherRequest { Id = other.Id })).ClassroomCount.ShouldBe(1);

        await Classrooms.DeleteClassroomAsync(new DeleteClassroomRequest { Id = created.Id });
        (await Classrooms.ListClassroomsAsync(new ListClassroomsRequest())).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Student_crud()
    {
        var classroom = await CreateClassroomAsync((await CreateTeacherAsync()).Id);
        var created = await CreateStudentAsync(classroom.Id);
        created.Address.ShouldBe("Hà Nội");
        created.Classroom.Teacher.FullName.ShouldBe("Trần Thị Bình");

        (await Students.GetStudentAsync(new GetStudentRequest { Id = created.Id })).ShouldBe(created);
        (await Students.GetStudentByCodeAsync(new GetStudentByCodeRequest { Code = "sv001" })).Id.ShouldBe(created.Id);

        // No address in the request = clear it (proto3 optional distinguishes "unset" from "").
        var updated = await Students.UpdateStudentAsync(new UpdateStudentRequest
        {
            Id = created.Id, Version = created.Version, Code = "SV001", FullName = "Nguyễn Văn Ánh",
            DateOfBirth = Date(2005, 1, 31), ClassroomId = classroom.Id,
        });
        updated.HasAddress.ShouldBeFalse();
        updated.FullName.ShouldBe("Nguyễn Văn Ánh");
        updated.Version.ShouldBe(2);

        var list = await Students.ListStudentsAsync(new ListStudentsRequest
        {
            CodePrefix = "SV", ClassroomId = classroom.Id, SortBy = StudentSortField.Name, Direction = Contracts.V1.SortDirection.Desc,
        });
        list.Items.ShouldHaveSingleItem().Id.ShouldBe(created.Id);
        (await Classrooms.GetClassroomAsync(new GetClassroomRequest { Id = classroom.Id })).StudentCount.ShouldBe(1);

        await Students.DeleteStudentAsync(new DeleteStudentRequest { Id = created.Id });
        (await Students.ListStudentsAsync(new ListStudentsRequest())).PageInfo.TotalCount.ShouldBe(0);
    }
}
