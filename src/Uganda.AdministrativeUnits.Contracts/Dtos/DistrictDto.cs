namespace Uganda.AdministrativeUnits.Contracts.Dtos;

/// <summary>District with child counts.</summary>
public sealed class DistrictDto
{
    /// <summary>Nationwide unique code (persist this value).</summary>
    public required string Code { get; init; }

    public required string Name { get; init; }

    public int ConstituencyCount { get; init; }
}
