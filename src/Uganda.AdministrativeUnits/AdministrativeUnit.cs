using System;
using System.Collections.Generic;
using Uganda.AdministrativeUnits.Internal;

namespace Uganda.AdministrativeUnits;

/// <summary>
/// Base type for every unit in the administrative hierarchy:
/// <see cref="District"/> → <see cref="Constituency"/> → <see cref="Subcounty"/> → <see cref="Parish"/> → <see cref="Village"/>.
/// Instances are immutable singletons owned by an <see cref="IAdministrativeUnitDirectory"/>.
/// </summary>
public abstract class AdministrativeUnit : IEquatable<AdministrativeUnit>
{
    private protected AdministrativeUnit(AdministrativeLevel level, string code, string name, AdministrativeUnit? parent)
    {
        Level = level;
        Code = code;
        Name = name;
        Parent = parent;
        FullCode = parent is null ? code : string.Concat(parent.FullCode, "-", code);
        NormalizedName = NameNormalizer.Normalize(name);
    }

    /// <summary>The tier of this unit.</summary>
    public AdministrativeLevel Level { get; }

    /// <summary>The code exactly as printed in the source register. Unique only among siblings (e.g. "01").</summary>
    public string Code { get; }

    /// <summary>
    /// A code that is unique across the whole country: the codes of this unit and all of its ancestors,
    /// joined with hyphens (e.g. <c>06-028-01-01-01</c> for a village).
    /// </summary>
    public string FullCode { get; }

    /// <summary>The unit's name as printed in the source register (upper case).</summary>
    public string Name { get; }

    /// <summary>The parent unit, or <see langword="null"/> for a <see cref="District"/>.</summary>
    public AdministrativeUnit? Parent { get; }

    /// <summary>
    /// The path from the district down to this unit, e.g.
    /// <c>HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA › KASAMBYA I</c>.
    /// </summary>
    public string Breadcrumb => Parent is null ? Name : string.Concat(Parent.Breadcrumb, " › ", Name);

    internal string NormalizedName { get; }

    /// <summary>Enumerates the ancestors of this unit, nearest first.</summary>
    public IEnumerable<AdministrativeUnit> Ancestors()
    {
        for (AdministrativeUnit? p = Parent; p is not null; p = p.Parent)
        {
            yield return p;
        }
    }

    /// <inheritdoc />
    public bool Equals(AdministrativeUnit? other) =>
        other is not null && Level == other.Level && string.Equals(FullCode, other.FullCode, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AdministrativeUnit);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Level, FullCode);

    /// <summary>Returns <see cref="Name"/>.</summary>
    public override string ToString() => Name;
}
