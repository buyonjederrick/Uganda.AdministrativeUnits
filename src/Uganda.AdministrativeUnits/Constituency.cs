using System.Collections.Generic;
using System.Linq;

namespace Uganda.AdministrativeUnits;

/// <summary>A constituency (county or division) within a <see cref="District"/>.</summary>
public sealed class Constituency : AdministrativeUnit
{
    private readonly List<Subcounty> _subcounties = new();

    internal Constituency(string code, string name, District district)
        : base(AdministrativeLevel.Constituency, code, name, district)
    {
        District = district;
        Subcounties = _subcounties.AsReadOnly();
    }

    /// <summary>The district this constituency belongs to.</summary>
    public District District { get; }

    /// <summary>The subcounties, town councils and divisions in this constituency.</summary>
    public IReadOnlyList<Subcounty> Subcounties { get; }

    /// <summary>All parishes in this constituency.</summary>
    public IEnumerable<Parish> Parishes => Subcounties.SelectMany(s => s.Parishes);

    /// <summary>All villages in this constituency.</summary>
    public IEnumerable<Village> Villages => Parishes.SelectMany(p => p.Villages);

    internal void Add(Subcounty subcounty) => _subcounties.Add(subcounty);
}
