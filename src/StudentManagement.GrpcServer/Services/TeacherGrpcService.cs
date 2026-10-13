using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using StudentManagement.Application.Teachers;
using StudentManagement.Contracts.V1;
using StudentManagement.GrpcServer.Mapping;

namespace StudentManagement.GrpcServer.Services;

public sealed class TeacherGrpcService(ITeacherAppService teachers) : TeacherService.TeacherServiceBase
{
    public override async Task<ListTeachersResponse> ListTeachers(ListTeachersRequest request, ServerCallContext context)
    {
        var page = await teachers.ListAsync(request.ToQuery(), context.CancellationToken);
        return new ListTeachersResponse { Items = { page.Items.Select(t => t.ToProto()) }, PageInfo = page.ToPageInfo() };
    }

    public override async Task<Teacher> GetTeacher(GetTeacherRequest request, ServerCallContext context) =>
        (await teachers.GetAsync(request.Id, context.CancellationToken)).ToProto();

    public override async Task<Teacher> CreateTeacher(CreateTeacherRequest request, ServerCallContext context) =>
        (await teachers.CreateAsync(request.ToInput(), context.CancellationToken)).ToProto();

    public override async Task<Teacher> UpdateTeacher(UpdateTeacherRequest request, ServerCallContext context) =>
        (await teachers.UpdateAsync(request.Id, request.Version, request.ToInput(), context.CancellationToken)).ToProto();

    public override async Task<Empty> DeleteTeacher(DeleteTeacherRequest request, ServerCallContext context)
    {
        await teachers.DeleteAsync(request.Id, context.CancellationToken);
        return new Empty();
    }
}
