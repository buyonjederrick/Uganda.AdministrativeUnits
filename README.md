# Uganda.AdministrativeUnits

An offline, dependency-free .NET directory of Uganda's verified administrative units: **146 districts, 353 constituencies, 2,198 subcounties / town councils, 10,717 parishes and 71,230 villages**, with official codes, hierarchy navigation and name search.

- **Zero dependencies and no network calls.** The dataset (~490 KB compressed) is embedded in the assembly.
- **Immutable and thread-safe.** Parsed once on first use (~200 ms, ~24 MB), then every lookup is in-memory.
- **Navigable both ways.** `village.Parish.Subcounty.Constituency.District`, or `district.Villages`.
- **Nationally unique codes.** Source codes only repeat between siblings, so every unit also gets a `FullCode` such as `100-118-01-13-05`.
- **Verified.** Counts match the totals printed in the source register.

Targets `netstandard2.0` (.NET Framework 4.6.1+, .NET Core 2.0+) and `net8.0`.

Namespace: `Uganda.AdministrativeUnits`.

## Install

```
dotnet add package Uganda.AdministrativeUnits
```

On .NET Framework, NuGet resolves the `netstandard2.0` asset (requires **4.6.1** or newer).

```csharp
using Uganda.AdministrativeUnits;

IAdministrativeUnitDirectory directory = AdministrativeUnitDirectory.Default;
```

---

## Hierarchy

```
District → Constituency → Subcounty → Parish → Village
```

| Level | Type | Meaning |
|---|---|---|
| 1 | `District` | District (incl. capital city district) |
| 2 | `Constituency` | County or division |
| 3 | `Subcounty` | Subcounty, town council, division, or ward-level unit |
| 4 | `Parish` | Parish or ward |
| 5 | `Village` | Village, cell, or zone |

`Code` is unique only among siblings. **`FullCode`** joins ancestor codes with `-` and is unique nationwide — **persist `FullCode`**, not bare `Code`.

---

## Contracts (public types)

Everything a consumer can reference is listed below. Constructors are `internal`; you obtain instances from the directory.

### `AdministrativeLevel`

```csharp
public enum AdministrativeLevel
{
    District = 1,
    Constituency = 2,
    Subcounty = 3,
    Parish = 4,
    Village = 5,
}
```

### `DatasetInfo`

```csharp
public sealed record DatasetInfo(
    string Title,
    string Edition,
    DateOnly PublishedOn,
    string SourceNote,
    int CorrectionsApplied);
```

| Property | Meaning |
|---|---|
| `Title` | Source register title |
| `Edition` | e.g. `"July 2022"` |
| `PublishedOn` | Date the source was generated |
| `SourceNote` | How the data was obtained |
| `CorrectionsApplied` | Count of documented source-data fixes |

### `DatasetStatistics`

```csharp
public sealed record DatasetStatistics(
    int Districts,
    int Constituencies,
    int Subcounties,
    int Parishes,
    int Villages);
```

### `IAdministrativeUnitDirectory`

```csharp
public interface IAdministrativeUnitDirectory
{
    DatasetInfo Dataset { get; }
    DatasetStatistics Statistics { get; }

    IReadOnlyList<District> Districts { get; }           // ordered by code
    IReadOnlyList<Constituency> Constituencies { get; }
    IReadOnlyList<Subcounty> Subcounties { get; }
    IReadOnlyList<Parish> Parishes { get; }
    IReadOnlyList<Village> Villages { get; }

    District? GetDistrict(string code);
    District? GetDistrictByName(string name);
    AdministrativeUnit? GetByFullCode(string fullCode);

    IReadOnlyList<AdministrativeUnit> Search(
        string query,
        AdministrativeLevel? level = null,
        int maxResults = 25);
}
```

| Member | Behaviour |
|---|---|
| `GetDistrict` | By local code (e.g. `"100"`). Trims input. `null` if missing. Throws if `code` is `null`. |
| `GetDistrictByName` | Ignores case, diacritics, apostrophe variants, extra whitespace. `null` if missing. Throws if `name` is `null`. |
| `GetByFullCode` | Any level by nationwide code (e.g. `"100-118-01-13-05"`). Trims input. `null` if missing. Throws if `fullCode` is `null`. |
| `Search` | Ranked name search: exact → prefix → word-prefix → substring. Ties: level, then name, then `FullCode`. Empty/whitespace query → empty list. `maxResults` must be positive. |

### `AdministrativeUnitDirectory`

```csharp
public sealed class AdministrativeUnitDirectory : IAdministrativeUnitDirectory
{
    public static AdministrativeUnitDirectory Default { get; }

    public static AdministrativeUnitDirectory Load();

    // + all IAdministrativeUnitDirectory members
}
```

| Member | Behaviour |
|---|---|
| `Default` | Shared, lazily loaded singleton. Prefer this. |
| `Load()` | New independent instance (re-parses embedded JSON). Use only when you need isolation. |

### `AdministrativeUnit` (base)

```csharp
public abstract class AdministrativeUnit : IEquatable<AdministrativeUnit>
{
    public AdministrativeLevel Level { get; }
    public string Code { get; }              // local; sibling-unique only (e.g. "01")
    public string FullCode { get; }          // nationwide unique (e.g. "100-118-01-13-05")
    public string Name { get; }              // as in source (usually upper case)
    public AdministrativeUnit? Parent { get; } // null for District
    public string Breadcrumb { get; }        // e.g. "KALUNGU › KALUNGU WEST COUNTY › …"

    public IEnumerable<AdministrativeUnit> Ancestors(); // nearest parent first

    public bool Equals(AdministrativeUnit? other);      // Level + FullCode
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public override string ToString();                  // returns Name
}
```

### `District`

```csharp
public sealed class District : AdministrativeUnit
{
    public IReadOnlyList<Constituency> Constituencies { get; }
    public IEnumerable<Subcounty> Subcounties { get; }  // flattened
    public IEnumerable<Parish> Parishes { get; }        // flattened
    public IEnumerable<Village> Villages { get; }       // flattened
}
```

`Parent` is always `null`. `FullCode` equals `Code`.

### `Constituency`

```csharp
public sealed class Constituency : AdministrativeUnit
{
    public District District { get; }
    public IReadOnlyList<Subcounty> Subcounties { get; }
    public IEnumerable<Parish> Parishes { get; }        // flattened
    public IEnumerable<Village> Villages { get; }       // flattened
}
```

### `Subcounty`

```csharp
public sealed class Subcounty : AdministrativeUnit
{
    public Constituency Constituency { get; }
    public District District { get; }
    public IReadOnlyList<Parish> Parishes { get; }
    public IEnumerable<Village> Villages { get; }       // flattened
}
```

### `Parish`

```csharp
public sealed class Parish : AdministrativeUnit
{
    public Subcounty Subcounty { get; }
    public Constituency Constituency { get; }
    public District District { get; }
    public IReadOnlyList<Village> Villages { get; }
}
```

### `Village`

```csharp
public sealed class Village : AdministrativeUnit
{
    public Parish Parish { get; }
    public Subcounty Subcounty { get; }
    public Constituency Constituency { get; }
    public District District { get; }
}
```

---

## Use cases

### 1. Read dataset metadata and counts

```csharp
DatasetInfo info = directory.Dataset;
Console.WriteLine($"{info.Title} ({info.Edition}, {info.PublishedOn:yyyy-MM-dd})");
Console.WriteLine($"Corrections: {info.CorrectionsApplied}");

DatasetStatistics stats = directory.Statistics;
Console.WriteLine($"{stats.Districts} districts · {stats.Villages} villages");
```

### 2. List every unit at a level (national)

```csharp
foreach (District d in directory.Districts)
    Console.WriteLine($"{d.Code} {d.Name}");

int parishCount = directory.Parishes.Count;
```

### 3. Look up a district by code or name

```csharp
District? byCode = directory.GetDistrict("100");
District? byName = directory.GetDistrictByName("kalungu");
District? same = directory.GetDistrictByName("  KALUNGU  ");
```

### 4. Resolve any unit by `FullCode` (persist & restore)

```csharp
AdministrativeUnit? unit = directory.GetByFullCode("100-118-01-13-05");
if (unit is Village village)
{
    Console.WriteLine(village.Breadcrumb);
    // KALUNGU › KALUNGU WEST COUNTY › KALUNGU › KITAMBA › KITAMBA
    Console.WriteLine(village.District.Name);
}
```

### 5. Search names (ranked)

```csharp
foreach (AdministrativeUnit hit in directory.Search("kampala", maxResults: 5))
    Console.WriteLine($"[{hit.Level}] {hit.Breadcrumb} ({hit.FullCode})");

foreach (AdministrativeUnit hit in directory.Search("kyamuzizi", AdministrativeLevel.Village))
    Console.WriteLine(hit.Breadcrumb);

IReadOnlyList<AdministrativeUnit> parishes =
    directory.Search("kitamba", AdministrativeLevel.Parish);
```

### 6. Walk the hierarchy down

```csharp
District kalungu = directory.GetDistrict("100")!;

foreach (Constituency county in kalungu.Constituencies)
    foreach (Subcounty subcounty in county.Subcounties)
        foreach (Parish parish in subcounty.Parishes)
            foreach (Village village in parish.Villages)
                Console.WriteLine(village.FullCode);

// Flattened helpers
int villagesInDistrict = kalungu.Villages.Count();
IEnumerable<Parish> parishesInCounty = county.Parishes;
IEnumerable<Village> villagesInSubcounty = subcounty.Villages;
```

### 7. Walk the hierarchy up (typed parents)

```csharp
Village village = (Village)directory.GetByFullCode("100-118-01-13-05")!;

Console.WriteLine(village.Level);        // Village
Console.WriteLine(village.Code);         // 05
Console.WriteLine(village.FullCode);     // 100-118-01-13-05
Console.WriteLine(village.Name);         // KITAMBA
Console.WriteLine(village.Parent!.Name); // KITAMBA (parish)
Console.WriteLine(village.Breadcrumb);
// KALUNGU › KALUNGU WEST COUNTY › KALUNGU › KITAMBA › KITAMBA

Console.WriteLine(village.Parish.Name);
Console.WriteLine(village.Subcounty.Name);
Console.WriteLine(village.Constituency.Name);
Console.WriteLine(village.District.Name);
// or: village.Parish.Subcounty.Constituency.District
```

### 8. Enumerate ancestors

Nearest parent first.

```csharp
foreach (AdministrativeUnit ancestor in village.Ancestors())
    Console.WriteLine($"{ancestor.Level}: {ancestor.Name}");
```

### 9. Cascade select (district → village)

Each dropdown lists **children of the selection above**. Bind lower levels by **`FullCode`**. When a parent changes, clear every deeper selection. Persist the deepest unit’s `FullCode`.

```csharp
IReadOnlyList<District> districts = directory.Districts;

District? district = directory.GetDistrict(selectedDistrictCode);
IReadOnlyList<Constituency> constituencies =
    district?.Constituencies ?? Array.Empty<Constituency>();

Constituency? constituency = constituencies
    .FirstOrDefault(c => c.FullCode == selectedConstituencyFullCode);
IReadOnlyList<Subcounty> subcounties =
    constituency?.Subcounties ?? Array.Empty<Subcounty>();

Subcounty? subcounty = subcounties
    .FirstOrDefault(s => s.FullCode == selectedSubcountyFullCode);
IReadOnlyList<Parish> parishes =
    subcounty?.Parishes ?? Array.Empty<Parish>();

Parish? parish = parishes
    .FirstOrDefault(p => p.FullCode == selectedParishFullCode);
IReadOnlyList<Village> villages =
    parish?.Villages ?? Array.Empty<Village>();

Village? village = villages
    .FirstOrDefault(v => v.FullCode == selectedVillageFullCode);

string? persist = village?.FullCode;
```

Restore a saved selection:

```csharp
if (directory.GetByFullCode(storedFullCode) is Village v)
{
    string districtCode = v.District.Code;
    string constituencyFullCode = v.Constituency.FullCode;
    string subcountyFullCode = v.Subcounty.FullCode;
    string parishFullCode = v.Parish.FullCode;
    string villageFullCode = v.FullCode;
}
```

You can stop at any level; still persist that level’s `FullCode`.

### 10. Validate a submitted address code

```csharp
AdministrativeUnit? unit = directory.GetByFullCode(request.FullCode);
if (unit is null)
    throw new ValidationException("Unknown administrative unit.");

if (unit.Level != AdministrativeLevel.Village)
    throw new ValidationException("Expected a village FullCode.");
```

### 11. Project to your own DTOs (do not serialize units)

The object graph has parent links (cycles). Map to flat models for APIs and persistence.

```csharp
public sealed record AddressDto(
    string FullCode,
    string Name,
    string Breadcrumb,
    AdministrativeLevel Level);

AddressDto dto = new(unit.FullCode, unit.Name, unit.Breadcrumb, unit.Level);
AdministrativeUnit? restored = directory.GetByFullCode(dto.FullCode);
```

### 12. Compare units

Equality is by `Level` + `FullCode` (ordinal).

```csharp
bool same = unitA.Equals(unitB);
string label = unitA.ToString(); // Name
```

### 13. Isolated directory instance

```csharp
AdministrativeUnitDirectory isolated = AdministrativeUnitDirectory.Load();
// independent parse of the embedded dataset; prefer Default in apps
```

---

## Design notes

| Topic | Decision |
|---|---|
| Hierarchy | Same nesting as the source register. |
| Names | Kept verbatim (usually upper case). Lookups/search fold case, Latin diacritics, apostrophe variants, whitespace. |
| Codes | `Code` = local. `FullCode` = ancestor path with `-`; unique nationwide. |
| Subcounty | Source “SUBCOUNTY/TOWN” → type `Subcounty`. |
| Search cost | O(n) scan with a bounded top-K heap. |
| Thread safety | After load, immutable and safe to share across threads. |
| Serialization | Do not serialize units; project to DTOs and restore via `GetByFullCode`. |

## Data and licence

Data is from *Uganda's Verified Administrative Units, July 2022* (generated 19 July 2022). This package is a snapshot of that edition, not a live registry.

The MIT licence covers this package's **code**. The underlying data belongs to its publisher; confirm you may redistribute it before publishing derivatives publicly.
