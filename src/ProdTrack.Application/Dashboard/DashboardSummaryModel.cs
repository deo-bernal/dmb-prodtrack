using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.Dashboard;

public sealed record DashboardSummaryModel(
    IReadOnlyDictionary<WorkOrderStatus, int> WorkOrdersByStatus,
    int LateCount,
    IReadOnlyList<StationWipModel> WipByStation);

public sealed record StationWipModel(int StationId, string StationCode, string StationName, bool IsActive, int Ready, int InProgress, int Paused, int Pending);
