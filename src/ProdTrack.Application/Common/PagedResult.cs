namespace ProdTrack.Application.Common;

/// <summary>Page of results: { items, page, pageSize, totalCount } (docs/02 section 6).</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
