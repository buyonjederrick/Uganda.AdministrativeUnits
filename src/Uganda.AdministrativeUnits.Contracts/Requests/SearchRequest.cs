using Uganda.AdministrativeUnits.Contracts.Enums;

namespace Uganda.AdministrativeUnits.Contracts.Requests;

/// <summary>Name search query parameters.</summary>
public sealed class SearchRequest
{
    public const int DefaultMaxResults = 25;
    public const int MaxAllowedResults = 100;

    public string Query { get; set; } = string.Empty;

    /// <summary>Optional level filter. When null, searches all levels.</summary>
    public AdministrativeLevel? Level { get; set; }

    /// <summary>
    /// Optional parent code. Restricts matches to descendants of that unit
    /// (e.g. search villages under a parish while typing in a cascade select).
    /// </summary>
    public string? ParentCode { get; set; }

    public int MaxResults { get; set; } = DefaultMaxResults;

    public void Normalize()
    {
        Query = Query?.Trim() ?? string.Empty;
        ParentCode = string.IsNullOrWhiteSpace(ParentCode) ? null : ParentCode.Trim();

        if (MaxResults < 1)
        {
            MaxResults = DefaultMaxResults;
        }

        if (MaxResults > MaxAllowedResults)
        {
            MaxResults = MaxAllowedResults;
        }
    }
}
