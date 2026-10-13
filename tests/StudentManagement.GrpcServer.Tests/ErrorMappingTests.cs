using Google.Rpc;
using Grpc.Core;
using StudentManagement.Contracts.V1;

namespace StudentManagement.GrpcServer.Tests;

/// <summary>One test per row of the error table (PROJECT_PLAN.md §4.6), end to end.</summary>
[Collection(GrpcServerCollection.Name)]
public class ErrorMappingTests(GrpcServerFixture fixture) : GrpcTest(fixture)
{
    private static async Task<RpcException> FailsWith<T>(AsyncUnaryCall<T> call, StatusCode expected, string errorCode)
    {
        var exception = await Should.ThrowAsync<RpcException>(call.ResponseAsync);
        exception.StatusCode.ShouldBe(expected);
        exception.GetRpcStatus().ShouldNotBeNull().GetDetail<ErrorInfo>().ShouldNotBeNull().Reason.ShouldBe(errorCode);
        return exception;
    }

    [Fact]
    public async Task Validation_failure_is_InvalidArgument_with_one_violation_per_field()
    {
        var exception = await FailsWith(Students.CreateStudentAsync(new CreateStudentRequest
        {
            Code = "x", FullName = " ", DateOfBirth = Date(2006, 3, 15), ClassroomId = 0,
        }), StatusCode.InvalidArgument, "VALIDATION_FAILED");

        exception.Status.Detail.ShouldBe("Dữ liệu không hợp lệ.");
        var violations = exception.GetRpcStatus()!.GetDetail<BadRequest>().ShouldNotBeNull().FieldViolations;
        violations.Select(v => v.Field).ShouldBe(new[] { "code", "full_name", "classroom_id" });
        violations.Single(v => v.Field == "full_name").Description.ShouldBe("Họ tên không được để trống.");
    }

    [Fact]
    public async Task Impossible_date_is_InvalidArgument_on_that_field()
    {
        var exception = await FailsWith(Teachers.CreateTeacherAsync(new CreateTeacherRequest
        {
            Code = "GV001", FullName = "Lê Văn Cường", DateOfBirth = Date(1990, 2, 30),
        }), StatusCode.InvalidArgument, "VALIDATION_FAILED");

        exception.GetRpcStatus()!.GetDetail<BadRequest>()!.FieldViolations.ShouldHaveSingleItem().Field.ShouldBe("date_of_birth");
    }

    [Fact]
    public async Task Missing_record_is_NotFound()
    {
        var exception = await FailsWith(Students.GetStudentAsync(new GetStudentRequest { Id = 999 }),
            StatusCode.NotFound, "STUDENT_NOT_FOUND");
        exception.Status.Detail.ShouldBe("Không tìm thấy sinh viên.");
    }

    [Fact]
    public async Task Duplicate_code_is_AlreadyExists()
    {
        await CreateTeacherAsync("GV001");
        await FailsWith(Teachers.CreateTeacherAsync(new CreateTeacherRequest
        {
            Code = "GV001", FullName = "Lê Văn Cường", DateOfBirth = Date(1990, 1, 1),
        }), StatusCode.AlreadyExists, "TEACHER_CODE_DUPLICATED");
    }

    [Fact]
    public async Task Deleting_what_is_still_referenced_is_FailedPrecondition()
    {
        var teacher = await CreateTeacherAsync();
        var classroom = await CreateClassroomAsync(teacher.Id);
        await CreateStudentAsync(classroom.Id);

        await FailsWith(Classrooms.DeleteClassroomAsync(new DeleteClassroomRequest { Id = classroom.Id }),
            StatusCode.FailedPrecondition, "CLASSROOM_HAS_STUDENTS");
        await FailsWith(Teachers.DeleteTeacherAsync(new DeleteTeacherRequest { Id = teacher.Id }),
            StatusCode.FailedPrecondition, "TEACHER_HAS_CLASSROOMS");
    }

    [Fact]
    public async Task Saving_a_stale_version_is_Aborted()
    {
        var teacher = await CreateTeacherAsync();
        var edit = new UpdateTeacherRequest
        {
            Id = teacher.Id, Version = teacher.Version, Code = "GV001", FullName = "Trần Thị Bích", DateOfBirth = Date(1980, 5, 1),
        };
        await Teachers.UpdateTeacherAsync(edit);

        // A second user still holding version 1.
        await FailsWith(Teachers.UpdateTeacherAsync(edit), StatusCode.Aborted, "CONCURRENCY_CONFLICT");
    }
}
