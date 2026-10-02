using System.Collections.Generic;

namespace Uganda.AdministrativeUnits;

/// <summary>A parish (or ward) within a <see cref="Subcounty"/>.</summary>
public sealed class Parish : AdministrativeUnit
{
    private readonly List<Village> _villages = new();

    internal Parish(string code, string name, Subcounty subcounty)
        : base(AdministrativeLevel.Parish, code, name, subcounty)
    {
        Subcounty = subcounty;
        Villages = _villages.AsReadOnly();
    }

    /// <summary>The subcounty this parish belongs to.</summary>
    public Subcounty Subcounty { get; }

    /// <summary>The constituency this parish belongs to.</summary>
    public Constituency Constituency => Subcounty.Constituency;

    /// <summary>The district this parish belongs to.</summary>
    public District District => Subcounty.District;

    /// <summary>The villages in this parish.</summary>
    public IReadOnlyList<Village> Villages { get; }

    internal void Add(Village village) => _villages.Add(village);
}
