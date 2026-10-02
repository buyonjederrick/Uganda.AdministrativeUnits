using System.Collections.Generic;
using System.Linq;

namespace Uganda.AdministrativeUnits;

/// <summary>
/// A subcounty, town council, division or ward-level unit within a <see cref="Constituency"/>
/// (the source register lists these together as "SUBCOUNTY/TOWN").
/// </summary>
public sealed class Subcounty : AdministrativeUnit
{
    private readonly List<Parish> _parishes = new();

    internal Subcounty(string code, string name, Constituency constituency)
        : base(AdministrativeLevel.Subcounty, code, name, constituency)
    {
        Constituency = constituency;
        Parishes = _parishes.AsReadOnly();
    }

    /// <summary>The constituency this subcounty belongs to.</summary>
    public Constituency Constituency { get; }

    /// <summary>The district this subcounty belongs to.</summary>
    public District District => Constituency.District;

    /// <summary>The parishes in this subcounty.</summary>
    public IReadOnlyList<Parish> Parishes { get; }

    /// <summary>All villages in this subcounty.</summary>
    public IEnumerable<Village> Villages => Parishes.SelectMany(p => p.Villages);

    internal void Add(Parish parish) => _parishes.Add(parish);
}
