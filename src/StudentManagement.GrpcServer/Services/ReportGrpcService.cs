using Grpc.Core;
using StudentManagement.Application.Reports;
using StudentManagement.Contracts.V1;
using StudentManagement.GrpcServer.Mapping;

namespace StudentManagement.GrpcServer.Services;

public sealed class ReportGrpcService(IReportAppService reports) : ReportService.ReportServiceBase
{
    public override async Task<DashboardResponse> GetDashboard(GetDashboardRequest request, ServerCallContext context) =>
        (await reports.GetDashboardAsync(context.CancellationToken)).ToProto();
}
