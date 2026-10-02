using System.Collections.Generic;
using System.Linq;

namespace Uganda.AdministrativeUnits;

/// <summary>A district. The top level of the hierarchy.</summary>
public sealed class District : AdministrativeUnit
{
    private readonly List<Constituency> _constituencies = new();

    internal District(string code, string name)
        : base(AdministrativeLevel.District, code, name, parent: null)
    {
        Constituencies = _constituencies.AsReadOnly();
    }

    /// <summary>The constituencies in this district.</summary>
    public IReadOnlyList<Constituency> Constituencies { get; }

    /// <summary>All subcounties in this district, across every constituency.</summary>
    public IEnumerable<Subcounty> Subcounties => Constituencies.SelectMany(c => c.Subcounties);

    /// <summary>All parishes in this district.</summary>
    public IEnumerable<Parish> Parishes => Subcounties.SelectMany(s => s.Parishes);

    /// <summary>All villages in this district.</summary>
    public IEnumerable<Village> Villages => Parishes.SelectMany(p => p.Villages);

    internal void Add(Constituency constituency) => _constituencies.Add(constituency);
}
