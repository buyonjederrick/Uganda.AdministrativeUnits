using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using Uganda.AdministrativeUnits.Internal;

namespace Uganda.AdministrativeUnits;

/// <summary>
/// The default <see cref="IAdministrativeUnitDirectory"/>, backed by the dataset embedded in this assembly.
/// The data is parsed once, on first use, and the resulting object graph is immutable and thread-safe.
/// </summary>
public sealed class AdministrativeUnitDirectory : IAdministrativeUnitDirectory
{
    private static readonly Lazy<AdministrativeUnitDirectory> SharedInstance =
        new Lazy<AdministrativeUnitDirectory>(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly ReadOnlyCollection<District> _districts;
    private readonly ReadOnlyCollection<Constituency> _constituencies;
    private readonly ReadOnlyCollection<Subcounty> _subcounties;
    private readonly ReadOnlyCollection<Parish> _parishes;
    private readonly ReadOnlyCollection<Village> _villages;
    private readonly Dictionary<string, District> _districtsByCode;
    private readonly Dictionary<string, District> _districtsByName;
    private readonly Dictionary<string, AdministrativeUnit> _byFullCode;

    private AdministrativeUnitDirectory(DatasetDto dto)
    {
        Dataset = new DatasetInfo(
            dto.Dataset.Title,
            dto.Dataset.Edition,
            DateOnly.Parse(dto.Dataset.PublishedOn, CultureInfo.InvariantCulture),
            dto.Dataset.SourceNote,
            dto.Dataset.CorrectionsApplied);

        var districts = new List<District>(dto.Districts.Count);
        var constituencies = new List<Constituency>();
        var subcounties = new List<Subcounty>();
        var parishes = new List<Parish>();
        var villages = new List<Village>();
        _byFullCode = new Dictionary<string, AdministrativeUnit>(StringComparer.Ordinal);
        _districtsByCode = new Dictionary<string, District>(StringComparer.Ordinal);
        _districtsByName = new Dictionary<string, District>(StringComparer.Ordinal);

        foreach (DistrictDto dd in dto.Districts)
        {
            var district = new District(dd.Code, dd.Name);
            Register(district);
            districts.Add(district);
            if (!_districtsByCode.TryAddValue(district.Code, district) || !_districtsByName.TryAddValue(district.NormalizedName, district))
            {
                throw new InvalidDataException($"Duplicate district '{district.Code} {district.Name}' in the embedded dataset.");
            }

            foreach (ConstituencyDto cd in dd.Constituencies)
            {
                var constituency = new Constituency(cd.Code, cd.Name, district);
                Register(constituency);
                district.Add(constituency);
                constituencies.Add(constituency);

                foreach (SubcountyDto sd in cd.Subcounties)
                {
                    var subcounty = new Subcounty(sd.Code, sd.Name, constituency);
                    Register(subcounty);
                    constituency.Add(subcounty);
                    subcounties.Add(subcounty);

                    foreach (ParishDto pd in sd.Parishes)
                    {
                        var parish = new Parish(pd.Code, pd.Name, subcounty);
                        Register(parish);
                        subcounty.Add(parish);
                        parishes.Add(parish);

                        foreach (UnitDto vd in pd.Villages)
                        {
                            var village = new Village(vd.Code, vd.Name, parish);
                            Register(village);
                            parish.Add(village);
                            villages.Add(village);
                        }
                    }
                }
            }
        }

        _districts = districts.AsReadOnly();
        _constituencies = constituencies.AsReadOnly();
        _subcounties = subcounties.AsReadOnly();
        _parishes = parishes.AsReadOnly();
        _villages = villages.AsReadOnly();
        Statistics = new DatasetStatistics(districts.Count, constituencies.Count, subcounties.Count, parishes.Count, villages.Count);
    }

    /// <summary>The shared, lazily created instance. Prefer this (or DI) over <see cref="Load"/>.</summary>
    public static AdministrativeUnitDirectory Default => SharedInstance.Value;

    /// <inheritdoc />
    public DatasetInfo Dataset { get; }

    /// <inheritdoc />
    public DatasetStatistics Statistics { get; }

    /// <inheritdoc />
    public IReadOnlyList<District> Districts => _districts;

    /// <inheritdoc />
    public IReadOnlyList<Constituency> Constituencies => _constituencies;

    /// <inheritdoc />
    public IReadOnlyList<Subcounty> Subcounties => _subcounties;

    /// <inheritdoc />
    public IReadOnlyList<Parish> Parishes => _parishes;

    /// <inheritdoc />
    public IReadOnlyList<Village> Villages => _villages;

    /// <summary>
    /// Creates a new, independent directory from the embedded dataset. This re-parses ~3 MB of JSON;
    /// use <see cref="Default"/> unless you specifically need an isolated instance.
    /// </summary>
    public static AdministrativeUnitDirectory Load() => new AdministrativeUnitDirectory(DatasetLoader.LoadEmbedded());

    /// <inheritdoc />
    public District? GetDistrict(string code)
    {
        Guard.NotNull(code, nameof(code));
        return _districtsByCode.GetValueOrDefaultCompat(code.Trim());
    }

    /// <inheritdoc />
    public District? GetDistrictByName(string name)
    {
        Guard.NotNull(name, nameof(name));
        return _districtsByName.GetValueOrDefaultCompat(NameNormalizer.Normalize(name));
    }

    /// <inheritdoc />
    public AdministrativeUnit? GetByFullCode(string fullCode)
    {
        Guard.NotNull(fullCode, nameof(fullCode));
        return _byFullCode.GetValueOrDefaultCompat(fullCode.Trim());
    }

    /// <inheritdoc />
    public IReadOnlyList<AdministrativeUnit> Search(string query, AdministrativeLevel? level = null, int maxResults = 25)
    {
        Guard.NotNull(query, nameof(query));
        Guard.Positive(maxResults, nameof(maxResults));

        string needle = NameNormalizer.Normalize(query);
        if (needle.Length == 0)
        {
            return Array.Empty<AdministrativeUnit>();
        }

        // Bounded "worst-on-top" heap: O(n log maxResults) however common the query is.
        var heap = new BoundedWorstFirstHeap<Hit>(static (a, b) => Compare(a, b));
        if (level is null || level == AdministrativeLevel.District) Collect(_districts, needle, heap, maxResults);
        if (level is null || level == AdministrativeLevel.Constituency) Collect(_constituencies, needle, heap, maxResults);
        if (level is null || level == AdministrativeLevel.Subcounty) Collect(_subcounties, needle, heap, maxResults);
        if (level is null || level == AdministrativeLevel.Parish) Collect(_parishes, needle, heap, maxResults);
        if (level is null || level == AdministrativeLevel.Village) Collect(_villages, needle, heap, maxResults);

        var results = new AdministrativeUnit[heap.Count];
        for (int i = results.Length - 1; i >= 0; i--)
        {
            results[i] = heap.Dequeue().Unit; // worst is dequeued first, so fill from the back
        }

        return results;
    }

    private static void Collect<T>(IReadOnlyList<T> pool, string needle, BoundedWorstFirstHeap<Hit> heap, int maxResults)
        where T : AdministrativeUnit
    {
        for (int i = 0; i < pool.Count; i++)
        {
            T unit = pool[i];
            int rank = Rank(unit.NormalizedName, needle);
            if (rank < 0)
            {
                continue;
            }

            var hit = new Hit(rank, unit);
            if (heap.Count < maxResults)
            {
                heap.Enqueue(hit);
            }
            else if (heap.TryPeek(out Hit worst) && Compare(hit, worst) < 0)
            {
                heap.ReplaceRoot(hit);
            }
        }
    }

    /// <summary>Orders hits best-first: rank, then level, then name, then full code (fully deterministic).</summary>
    private static int Compare(Hit a, Hit b)
    {
        int c = a.Rank.CompareTo(b.Rank);
        if (c != 0)
        {
            return c;
        }

        c = a.Unit.Level.CompareTo(b.Unit.Level);
        if (c != 0)
        {
            return c;
        }

        c = string.CompareOrdinal(a.Unit.NormalizedName, b.Unit.NormalizedName);
        return c != 0 ? c : string.CompareOrdinal(a.Unit.FullCode, b.Unit.FullCode);
    }

    private readonly struct Hit
    {
        public Hit(int rank, AdministrativeUnit unit)
        {
            Rank = rank;
            Unit = unit;
        }

        public int Rank { get; }

        public AdministrativeUnit Unit { get; }
    }

    /// <summary>0 = exact, 1 = prefix, 2 = starts a word, 3 = substring, -1 = no match.</summary>
    private static int Rank(string name, string needle)
    {
        if (name.Length == needle.Length)
        {
            return string.Equals(name, needle, StringComparison.Ordinal) ? 0 : -1;
        }

        int index = name.IndexOf(needle, StringComparison.Ordinal);
        if (index < 0)
        {
            return -1;
        }

        if (index == 0)
        {
            return 1;
        }

        return name[index - 1] == ' ' ? 2 : 3;
    }

    private void Register(AdministrativeUnit unit)
    {
        if (!_byFullCode.TryAddValue(unit.FullCode, unit))
        {
            throw new InvalidDataException($"Duplicate full code '{unit.FullCode}' in the embedded dataset ({unit.Level}: {unit.Name}).");
        }
    }
}
