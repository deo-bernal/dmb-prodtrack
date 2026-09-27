namespace ProdTrack.Contracts.Common;

/// <summary>Paged list: { items, page, pageSize, totalCount }.</summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
