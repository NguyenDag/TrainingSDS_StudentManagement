namespace StudentManagement.Application.Reports;

/// <summary>
/// Read-only aggregates for the dashboard. Kept apart from the CRUD repositories (ISP):
/// reporting needs GROUP BY queries across tables, not entities.
/// </summary>
public interface IReportQueries
{
    /// <summary>All aggregates, computed by the database without loading rows into memory.</summary>
    Task<ReportData> GetAsync(CancellationToken cancellationToken = default);
}
