namespace ProdTrack.Contracts.Dashboard;

public sealed record StationWipDto(int StationId, string StationCode, string StationName, bool IsActive, int Ready, int InProgress, int Paused, int Pending);

public sealed record DashboardSummaryDto(IReadOnlyDictionary<string, int> WorkOrdersByStatus, int LateCount, IReadOnlyList<StationWipDto> WipByStation);
