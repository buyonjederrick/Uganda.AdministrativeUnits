namespace Uganda.AdministrativeUnits.Contracts.Requests;

/// <summary>Common paging query parameters.</summary>
public sealed class PagingRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    public int Page { get; set; } = DefaultPage;

    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>
    /// When true, returns the full result set in one response (ignores <see cref="Page"/> / <see cref="PageSize"/>).
    /// Use for cascade dropdowns so options are never truncated by paging.
    /// </summary>
    public bool GetAll { get; set; }

    public void Normalize()
    {
        if (Page < 1)
        {
            Page = DefaultPage;
        }

        if (PageSize < 1)
        {
            PageSize = DefaultPageSize;
        }

        if (PageSize > MaxPageSize)
        {
            PageSize = MaxPageSize;
        }
    }
}
