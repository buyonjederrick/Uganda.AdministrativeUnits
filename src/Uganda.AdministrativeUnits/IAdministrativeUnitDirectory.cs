using System.Collections.Generic;

namespace Uganda.AdministrativeUnits;

/// <summary>
/// Read-only, thread-safe access to Uganda's administrative units.
/// Obtain the shared instance from <see cref="AdministrativeUnitDirectory.Default"/> or via dependency injection.
/// </summary>
public interface IAdministrativeUnitDirectory
{
    /// <summary>Provenance of the underlying data.</summary>
    DatasetInfo Dataset { get; }

    /// <summary>Number of units at each level.</summary>
    DatasetStatistics Statistics { get; }

    /// <summary>All districts, ordered by code.</summary>
    IReadOnlyList<District> Districts { get; }

    /// <summary>All constituencies in the country.</summary>
    IReadOnlyList<Constituency> Constituencies { get; }

    /// <summary>All subcounties, town councils and divisions in the country.</summary>
    IReadOnlyList<Subcounty> Subcounties { get; }

    /// <summary>All parishes in the country.</summary>
    IReadOnlyList<Parish> Parishes { get; }

    /// <summary>All villages in the country.</summary>
    IReadOnlyList<Village> Villages { get; }

    /// <summary>Finds a district by its code (e.g. "06"). Returns <see langword="null"/> if there is none.</summary>
    District? GetDistrict(string code);

    /// <summary>
    /// Finds a district by name. Matching ignores case, diacritics and surrounding or repeated whitespace.
    /// Returns <see langword="null"/> if there is none.
    /// </summary>
    District? GetDistrictByName(string name);

    /// <summary>
    /// Finds any unit by its <see cref="AdministrativeUnit.FullCode"/> (e.g. "06-028-01-01-01").
    /// Returns <see langword="null"/> if there is none.
    /// </summary>
    AdministrativeUnit? GetByFullCode(string fullCode);

    /// <summary>
    /// Searches unit names. Matching ignores case, diacritics and repeated whitespace.
    /// Results are ranked: exact match, then prefix, then word-prefix, then substring; ties are broken by level, then name.
    /// </summary>
    /// <param name="query">The text to look for.</param>
    /// <param name="level">Restrict the search to one level, or <see langword="null"/> to search every level.</param>
    /// <param name="maxResults">Maximum number of results to return. Must be positive.</param>
    IReadOnlyList<AdministrativeUnit> Search(string query, AdministrativeLevel? level = null, int maxResults = 25);
}
