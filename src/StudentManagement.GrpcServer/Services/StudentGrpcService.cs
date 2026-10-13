using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using StudentManagement.Application.Students;
using StudentManagement.Contracts.V1;
using StudentManagement.GrpcServer.Mapping;

namespace StudentManagement.GrpcServer.Services;

/// <summary>Transport only: proto → DTO → use case → proto. Errors are mapped by ExceptionInterceptor.</summary>
public sealed class StudentGrpcService(IStudentAppService students) : StudentService.StudentServiceBase
{
    public override async Task<ListStudentsResponse> ListStudents(ListStudentsRequest request, ServerCallContext context)
    {
        var page = await students.ListAsync(request.ToQuery(), context.CancellationToken);
        return new ListStudentsResponse { Items = { page.Items.Select(s => s.ToProto()) }, PageInfo = page.ToPageInfo() };
    }

    public override async Task<Student> GetStudent(GetStudentRequest request, ServerCallContext context) =>
        (await students.GetAsync(request.Id, context.CancellationToken)).ToProto();

    public override async Task<Student> GetStudentByCode(GetStudentByCodeRequest request, ServerCallContext context) =>
        (await students.GetByCodeAsync(request.Code, context.CancellationToken)).ToProto();

    public override async Task<Student> CreateStudent(CreateStudentRequest request, ServerCallContext context) =>
        (await students.CreateAsync(request.ToInput(), context.CancellationToken)).ToProto();

    public override async Task<Student> UpdateStudent(UpdateStudentRequest request, ServerCallContext context) =>
        (await students.UpdateAsync(request.Id, request.Version, request.ToInput(), context.CancellationToken)).ToProto();

    public override async Task<Empty> DeleteStudent(DeleteStudentRequest request, ServerCallContext context)
    {
        await students.DeleteAsync(request.Id, context.CancellationToken);
        return new Empty();
    }
}
