using Uganda.AdministrativeUnits.Contracts.Enums;

namespace Uganda.AdministrativeUnits.Contracts.Dtos;

/// <summary>Lightweight projection of any administrative unit.</summary>
public sealed class UnitSummaryDto
{
    /// <summary>Nationwide unique code. Persist and use this for cascade / lookups.</summary>
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required AdministrativeLevel Level { get; init; }

    public string? ParentCode { get; init; }

    public required string Breadcrumb { get; init; }
}
