namespace Uganda.AdministrativeUnits;

/// <summary>A village (or cell/zone) within a <see cref="Parish"/>. The lowest level of the hierarchy.</summary>
public sealed class Village : AdministrativeUnit
{
    internal Village(string code, string name, Parish parish)
        : base(AdministrativeLevel.Village, code, name, parish)
    {
        Parish = parish;
    }

    /// <summary>The parish this village belongs to.</summary>
    public Parish Parish { get; }

    /// <summary>The subcounty this village belongs to.</summary>
    public Subcounty Subcounty => Parish.Subcounty;

    /// <summary>The constituency this village belongs to.</summary>
    public Constituency Constituency => Parish.Constituency;

    /// <summary>The district this village belongs to.</summary>
    public District District => Parish.District;
}
