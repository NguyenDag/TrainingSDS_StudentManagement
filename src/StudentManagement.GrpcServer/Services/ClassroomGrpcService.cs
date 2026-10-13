using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using StudentManagement.Application.Classrooms;
using StudentManagement.Contracts.V1;
using StudentManagement.GrpcServer.Mapping;

namespace StudentManagement.GrpcServer.Services;

public sealed class ClassroomGrpcService(IClassroomAppService classrooms) : ClassroomService.ClassroomServiceBase
{
    public override async Task<ListClassroomsResponse> ListClassrooms(ListClassroomsRequest request, ServerCallContext context)
    {
        var page = await classrooms.ListAsync(request.ToQuery(), context.CancellationToken);
        return new ListClassroomsResponse { Items = { page.Items.Select(c => c.ToProto()) }, PageInfo = page.ToPageInfo() };
    }

    public override async Task<Classroom> GetClassroom(GetClassroomRequest request, ServerCallContext context) =>
        (await classrooms.GetAsync(request.Id, context.CancellationToken)).ToProto();

    public override async Task<Classroom> CreateClassroom(CreateClassroomRequest request, ServerCallContext context) =>
        (await classrooms.CreateAsync(request.ToInput(), context.CancellationToken)).ToProto();

    public override async Task<Classroom> UpdateClassroom(UpdateClassroomRequest request, ServerCallContext context) =>
        (await classrooms.UpdateAsync(request.Id, request.Version, request.ToInput(), context.CancellationToken)).ToProto();

    public override async Task<Empty> DeleteClassroom(DeleteClassroomRequest request, ServerCallContext context)
    {
        await classrooms.DeleteAsync(request.Id, context.CancellationToken);
        return new Empty();
    }
}
