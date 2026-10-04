using System.Collections.Generic;

namespace Uganda.AdministrativeUnits.Contracts.Responses;

/// <summary>Paged collection of items.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
