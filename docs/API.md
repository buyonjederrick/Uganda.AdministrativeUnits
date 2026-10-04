# API usage guide

How to use every public type and member in `Uganda.AdministrativeUnits` and `Uganda.AdministrativeUnits.DependencyInjection`, including cascade select.

The object graph is **immutable and thread-safe** after first load. Prefer `AdministrativeUnitDirectory.Default` or DI; do not re-parse the dataset unless you need an isolated instance.

---

## Packages

| Package | Role |
|---|---|
| `Uganda.AdministrativeUnits` | Core directory, units, search. Zero dependencies. |
| `Uganda.AdministrativeUnits.DependencyInjection` | `AddUgandaAdministrativeUnits()` for ASP.NET / generic host. |

```
dotnet add package Uganda.AdministrativeUnits
dotnet add package Uganda.AdministrativeUnits.DependencyInjection   # optional
```

Targets: `netstandard2.0` (.NET Framework 4.6.1+, .NET Core 2.0+) and `net8.0`.

---

## Hierarchy

```
District → Constituency → Subcounty → Parish → Village
```

| Level | Type | Source meaning |
|---|---|---|
| 1 | `District` | District (incl. capital city district) |
| 2 | `Constituency` | County or division |
| 3 | `Subcounty` | Subcounty, town council, division, or ward-level unit |
| 4 | `Parish` | Parish or ward |
| 5 | `Village` | Village, cell, or zone |

`Code` is local among siblings. `FullCode` joins ancestor codes with `-` and is unique nationwide (e.g. `06-028-01-01-01`). **Persist `FullCode`**, not bare `Code`.

---

## Obtaining the directory

### `AdministrativeUnitDirectory.Default`

Shared, lazily loaded singleton. Parsed once on first access (~200 ms, ~24 MB), then every lookup is in-memory.

```csharp
using Uganda.AdministrativeUnits;

IAdministrativeUnitDirectory directory = AdministrativeUnitDirectory.Default;
```

### `AdministrativeUnitDirectory.Load()`

Creates a **new** independent directory and re-parses the embedded JSON. Use only when you need isolation from `Default`. Prefer `Default` or DI in application code.

```csharp
AdministrativeUnitDirectory isolated = AdministrativeUnitDirectory.Load();
```

### Dependency injection — `AddUgandaAdministrativeUnits()`

Registers `IAdministrativeUnitDirectory` as a singleton pointing at `AdministrativeUnitDirectory.Default`. Safe to call more than once (`TryAddSingleton`).

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddUgandaAdministrativeUnits();

// later
public sealed class AddressService(IAdministrativeUnitDirectory directory)
{
    public District? Find(string code) => directory.GetDistrict(code);
}
```

Blazor / MVC: inject `IAdministrativeUnitDirectory` into pages or controllers the same way.

---

## `IAdministrativeUnitDirectory`

All members below are available on `AdministrativeUnitDirectory` via the interface.

### `Dataset` → `DatasetInfo`

Provenance of the embedded register.

| Property | Meaning |
|---|---|
| `Title` | Source register title |
| `Edition` | e.g. `"July 2022"` |
| `PublishedOn` | `DateOnly` the source was generated |
| `SourceNote` | How the data was obtained |
| `CorrectionsApplied` | Count of documented source-data fixes |

```csharp
DatasetInfo info = directory.Dataset;
Console.WriteLine($"{info.Title} ({info.Edition}, {info.PublishedOn:yyyy-MM-dd})");
Console.WriteLine($"Corrections applied: {info.CorrectionsApplied}");
```

### `Statistics` → `DatasetStatistics`

Counts at each level.

| Property | Meaning |
|---|---|
| `Districts` | National district count |
| `Constituencies` | National constituency count |
| `Subcounties` | National subcounty / town / division count |
| `Parishes` | National parish count |
| `Villages` | National village count |

```csharp
DatasetStatistics s = directory.Statistics;
Console.WriteLine($"{s.Districts} districts · {s.Villages} villages");
```

### National collections

| Member | Returns |
|---|---|
| `Districts` | All districts, ordered by code |
| `Constituencies` | All constituencies |
| `Subcounties` | All subcounties / towns / divisions |
| `Parishes` | All parishes |
| `Villages` | All villages |

These are `IReadOnlyList<T>` over the full country. Prefer hierarchy navigation or search for UI lists; use national collections for reporting, indexes, or cascade **district** dropdowns.

```csharp
foreach (District d in directory.Districts)
    Console.WriteLine($"{d.Code} {d.Name}");

int parishCount = directory.Parishes.Count;
```

### `GetDistrict(string code)`

Looks up a district by local code (e.g. `"06"`). Trims input. Returns `null` if missing. Throws if `code` is `null`.

```csharp
District? hoima = directory.GetDistrict("06");
if (hoima is not null)
    Console.WriteLine(hoima.Name);
```

### `GetDistrictByName(string name)`

Looks up a district by name. Matching ignores case, Latin diacritics, apostrophe variants, and surrounding/repeated whitespace. Returns `null` if missing. Throws if `name` is `null`.

```csharp
District? hoima = directory.GetDistrictByName("hoima");
District? same = directory.GetDistrictByName("  HOIMA  ");
```

There is **no** name lookup for lower levels on the directory; use `Search`, walk children, or resolve `FullCode`.

### `GetByFullCode(string fullCode)`

Resolves any unit (any level) by nationwide `FullCode`. Trims input. Returns `null` if missing. Throws if `fullCode` is `null`.

```csharp
AdministrativeUnit? unit = directory.GetByFullCode("06-028-01-01-01");
if (unit is Village village)
{
    Console.WriteLine(village.Breadcrumb);
    Console.WriteLine(village.District.Name);
}
```

Use this when reloading a stored address from a database or form field.

### `Search(string query, AdministrativeLevel? level = null, int maxResults = 25)`

Ranked name search across the country (or one level).

| Parameter | Behaviour |
|---|---|
| `query` | Required. Normalized like name lookups. Empty/whitespace → empty list. |
| `level` | `null` = all levels; otherwise only that tier. |
| `maxResults` | Must be positive (default 25). Throws if ≤ 0. |

**Ranking (best first):**

1. Exact match on normalized name  
2. Prefix match  
3. Word-prefix (needle starts a word after a space)  
4. Substring  

Ties: lower `AdministrativeLevel` first, then name, then `FullCode` (deterministic).

Cost: O(n) scan with a bounded top-K heap — a few milliseconds even for short queries.

```csharp
// All levels, top 5
foreach (AdministrativeUnit hit in directory.Search("kampala", maxResults: 5))
    Console.WriteLine($"[{hit.Level}] {hit.Breadcrumb} ({hit.FullCode})");

// Villages only
foreach (AdministrativeUnit hit in directory.Search("kyamuzizi", AdministrativeLevel.Village))
    Console.WriteLine(hit.Breadcrumb);

// Parishes only
IReadOnlyList<AdministrativeUnit> parishes = directory.Search("katereiga", AdministrativeLevel.Parish);
```

---

## `AdministrativeLevel`

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

Use with `Search`, UI filters, and `unit.Level` checks / pattern matching.

```csharp
if (unit.Level == AdministrativeLevel.Village)
{
    var village = (Village)unit;
}
```

---

## `AdministrativeUnit` (base type)

Every district, constituency, subcounty, parish, and village is an `AdministrativeUnit`.

### Properties

| Member | Description |
|---|---|
| `Level` | `AdministrativeLevel` of this unit |
| `Code` | Source local code (sibling-unique only), e.g. `"01"` |
| `FullCode` | Nationwide unique path, e.g. `"06-028-01-01-01"` |
| `Name` | Source name (usually upper case) |
| `Parent` | Parent unit, or `null` for a district |
| `Breadcrumb` | Path from district down, joined with ` › ` |

```csharp
Village village = /* ... */;
Console.WriteLine(village.Level);       // Village
Console.WriteLine(village.Code);        // 01
Console.WriteLine(village.FullCode);    // 06-028-01-01-01
Console.WriteLine(village.Name);        // KASAMBYA I
Console.WriteLine(village.Parent!.Name); // parish name
Console.WriteLine(village.Breadcrumb);
// HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA › KASAMBYA I
```

### `Ancestors()`

Enumerates parents nearest-first (parish → subcounty → constituency → district for a village).

```csharp
foreach (AdministrativeUnit ancestor in village.Ancestors())
    Console.WriteLine($"{ancestor.Level}: {ancestor.Name}");
```

### Equality

`Equals` / `GetHashCode` compare `Level` + `FullCode` (ordinal). Same unit from `Default` is reference-equal as well, but equality is value-based on those fields so comparisons stay meaningful across instances from `Load()`.

```csharp
bool same = unitA.Equals(unitB);
bool sameOp = unitA == unitB; // not overloaded; use Equals
```

### `ToString()`

Returns `Name`.

### Serialization

Do **not** serialize the object graph (parent links create cycles). Project to your own DTOs:

```csharp
public sealed record AddressDto(string FullCode, string Name, string Breadcrumb, AdministrativeLevel Level);

AddressDto dto = new(unit.FullCode, unit.Name, unit.Breadcrumb, unit.Level);
// later:
AdministrativeUnit? restored = directory.GetByFullCode(dto.FullCode);
```

---

## `District`

Top level. `Parent` is always `null`. `FullCode` equals `Code`.

| Member | Type | Description |
|---|---|---|
| `Constituencies` | `IReadOnlyList<Constituency>` | Direct children |
| `Subcounties` | `IEnumerable<Subcounty>` | Flattened across constituencies |
| `Parishes` | `IEnumerable<Parish>` | Flattened across the district |
| `Villages` | `IEnumerable<Village>` | Flattened across the district |

```csharp
District hoima = directory.GetDistrict("06")!;

foreach (Constituency county in hoima.Constituencies)
    Console.WriteLine(county.Name);

int villageCount = hoima.Villages.Count(); // LINQ over flattened sequence
Village first = hoima.Villages.First();
```

---

## `Constituency`

| Member | Type | Description |
|---|---|---|
| `District` | `District` | Parent district (typed) |
| `Subcounties` | `IReadOnlyList<Subcounty>` | Direct children |
| `Parishes` | `IEnumerable<Parish>` | Flattened |
| `Villages` | `IEnumerable<Village>` | Flattened |

```csharp
Constituency county = hoima.Constituencies[0];
Console.WriteLine(county.District.Name);
foreach (Subcounty sc in county.Subcounties)
    Console.WriteLine($"{sc.Name}: {sc.Parishes.Count} parishes");
```

---

## `Subcounty`

Covers subcounties, town councils, divisions, and similar units listed under “SUBCOUNTY/TOWN” in the source.

| Member | Type | Description |
|---|---|---|
| `Constituency` | `Constituency` | Parent constituency |
| `District` | `District` | Via constituency |
| `Parishes` | `IReadOnlyList<Parish>` | Direct children |
| `Villages` | `IEnumerable<Village>` | Flattened |

```csharp
Subcounty subcounty = county.Subcounties[0];
Console.WriteLine(subcounty.District.Name);
Console.WriteLine(subcounty.Constituency.Name);
foreach (Parish parish in subcounty.Parishes)
    Console.WriteLine($"{parish.Name}: {parish.Villages.Count} villages");
```

---

## `Parish`

| Member | Type | Description |
|---|---|---|
| `Subcounty` | `Subcounty` | Parent |
| `Constituency` | `Constituency` | Via subcounty |
| `District` | `District` | Via subcounty |
| `Villages` | `IReadOnlyList<Village>` | Direct children |

```csharp
Parish parish = subcounty.Parishes[0];
Console.WriteLine(parish.District.Name);
foreach (Village v in parish.Villages)
    Console.WriteLine($"{v.FullCode} {v.Name}");
```

---

## `Village`

Lowest level.

| Member | Type | Description |
|---|---|---|
| `Parish` | `Parish` | Parent |
| `Subcounty` | `Subcounty` | Via parish |
| `Constituency` | `Constituency` | Via parish |
| `District` | `District` | Via parish |

```csharp
Village village = parish.Villages[0];
Console.WriteLine(village.Parish.Name);
Console.WriteLine(village.Subcounty.Name);
Console.WriteLine(village.Constituency.Name);
Console.WriteLine(village.District.Name);
// or climb: village.Parish.Subcounty.Constituency.District
```

---

## Walking the hierarchy

### Down (browse / menus)

```csharp
foreach (District district in directory.Districts)
    foreach (Constituency county in district.Constituencies)
        foreach (Subcounty subcounty in county.Subcounties)
            foreach (Parish parish in subcounty.Parishes)
                foreach (Village village in parish.Villages)
                    Console.WriteLine(village.FullCode);
```

### Up (from a stored village)

```csharp
AdministrativeUnit? unit = directory.GetByFullCode(storedFullCode);
if (unit is Village village)
{
    Console.WriteLine(village.Breadcrumb);
    Console.WriteLine(village.District.Name);
}
```

### Mixed (district → sample village)

```csharp
District? d = directory.GetDistrictByName("kampala");
if (d is not null)
{
    Village sample = d.Villages.First();
    Console.WriteLine(sample.Breadcrumb);
}
```

---

## Cascade select (district → village)

Use this pattern for address pickers: each dropdown lists **children of the selection above**. When a parent changes, clear all deeper selections.

### Rules

1. **District list:** `directory.Districts` (bind value to `district.Code` or `district.FullCode` — they are the same).
2. **Lower lists:** children of the selected parent (`district.Constituencies`, `constituency.Subcounties`, …).
3. **Bind lower levels by `FullCode`**, not bare `Code` (codes repeat across the country).
4. On parent change, reset every deeper field to empty / `null`.
5. Resolve the final selection with `GetByFullCode` or by matching `FullCode` in the current child list.
6. **Persist** the village (or deepest chosen unit) `FullCode`.

### Console / service sketch

```csharp
IAdministrativeUnitDirectory directory = AdministrativeUnitDirectory.Default;

// 1) Districts
IReadOnlyList<District> districts = directory.Districts;

// User picks district code "06"
District? district = directory.GetDistrict("06");
if (district is null) return;

// 2) Constituencies for that district
IReadOnlyList<Constituency> constituencies = district.Constituencies;

// User picks a constituency FullCode
Constituency? constituency = constituencies
    .FirstOrDefault(c => c.FullCode == selectedConstituencyFullCode);
if (constituency is null) return;

// 3–5) Same pattern down the tree
IReadOnlyList<Subcounty> subcounties = constituency.Subcounties;
Subcounty? subcounty = subcounties.FirstOrDefault(s => s.FullCode == selectedSubcountyFullCode);
if (subcounty is null) return;

IReadOnlyList<Parish> parishes = subcounty.Parishes;
Parish? parish = parishes.FirstOrDefault(p => p.FullCode == selectedParishFullCode);
if (parish is null) return;

IReadOnlyList<Village> villages = parish.Villages;
Village? village = villages.FirstOrDefault(v => v.FullCode == selectedVillageFullCode);
if (village is null) return;

// Store this:
string persist = village.FullCode;
string display = village.Breadcrumb;
```

### Blazor (same idea as the sample at `/cascade`)

Live demo: run the Blazor sample and open `/cascade` (see [DEVELOPMENT.md](DEVELOPMENT.md)).

```razor
@inject IAdministrativeUnitDirectory Directory

<select @bind="_districtCode" @bind:after="OnDistrictChanged">
    <option value="">— District —</option>
    @foreach (District d in Directory.Districts)
    {
        <option value="@d.Code">@d.Name</option>
    }
</select>

<select @bind="_constituencyFullCode" @bind:after="OnConstituencyChanged"
        disabled="@(_selectedDistrict is null)">
    <option value="">— Constituency —</option>
    @if (_selectedDistrict is not null)
    {
        @foreach (Constituency c in _selectedDistrict.Constituencies)
        {
            <option value="@c.FullCode">@c.Name</option>
        }
    }
</select>

@* Repeat for Subcounty → Parish → Village, each bound to FullCode *@

@code {
    string _districtCode = "";
    string _constituencyFullCode = "";
    // ... subcounty, parish, village FullCode fields

    District? _selectedDistrict;
    Constituency? _selectedConstituency;
    // ...

    void OnDistrictChanged()
    {
        _constituencyFullCode = "";
        // clear subcounty, parish, village FullCodes too
        Sync();
    }

    void OnConstituencyChanged()
    {
        // clear subcounty, parish, village
        Sync();
    }

    void Sync()
    {
        _selectedDistrict = string.IsNullOrEmpty(_districtCode)
            ? null
            : Directory.GetDistrict(_districtCode);

        _selectedConstituency = Resolve(_selectedDistrict?.Constituencies, _constituencyFullCode);
        // Resolve subcounty from constituency.Subcounties, etc.
    }

    static T? Resolve<T>(IReadOnlyList<T>? children, string fullCode)
        where T : AdministrativeUnit
    {
        if (children is null || string.IsNullOrEmpty(fullCode))
            return null;

        for (int i = 0; i < children.Count; i++)
        {
            if (string.Equals(children[i].FullCode, fullCode, StringComparison.Ordinal))
                return children[i];
        }

        return null;
    }
}
```

### Restoring a cascade from a stored `FullCode`

```csharp
AdministrativeUnit? unit = directory.GetByFullCode(storedFullCode);
if (unit is not Village village)
    return; // or handle partial levels

string districtCode = village.District.Code;
string constituencyFullCode = village.Constituency.FullCode;
string subcountyFullCode = village.Subcounty.FullCode;
string parishFullCode = village.Parish.FullCode;
string villageFullCode = village.FullCode;

// Bind those into the five dropdowns; child lists come from the parents as above.
```

### Partial cascade (stop before village)

You can stop at any level (e.g. district-only filter). Still persist that level’s `FullCode` and resolve with `GetByFullCode`.

---

## End-to-end recipes

### 1. Validate a user-submitted address code

```csharp
AdministrativeUnit? unit = directory.GetByFullCode(request.FullCode);
if (unit is null)
    throw new ValidationException("Unknown administrative unit.");

if (unit.Level != AdministrativeLevel.Village)
    throw new ValidationException("Expected a village FullCode.");
```

### 2. Type-ahead search box

```csharp
IReadOnlyList<AdministrativeUnit> hits =
    directory.Search(userText, level: null, maxResults: 20);

// Show hit.Breadcrumb + hit.FullCode; on pick, store hit.FullCode
```

### 3. District report

```csharp
District? d = directory.GetDistrictByName(name);
if (d is null) return;

Console.WriteLine($"{d.Name}: {d.Constituencies.Count} constituencies");
Console.WriteLine($"Villages: {d.Villages.Count()}");
```

### 4. Project for APIs / UI (avoid serializing units)

```csharp
var dto = new
{
    unit.FullCode,
    unit.Name,
    unit.Level,
    unit.Breadcrumb,
    District = unit is Village v ? v.District.Name : null,
};
```

---

## Sample apps (every API surface exercised)

| Sample | Path / command | APIs demonstrated |
|---|---|---|
| Console | `samples/Uganda.AdministrativeUnits.Sample` | `Dataset`, `Statistics`, `GetDistrict`, `GetDistrictByName`, hierarchy walk, `Breadcrumb`, `FullCode`, parent navigation, `GetByFullCode`, `Search` |
| Blazor Overview `/` | `Dataset`, `Statistics`, `GetDistrict`, `Villages`, `Breadcrumb` |
| Blazor Districts `/districts` | `GetDistrict`, `GetDistrictByName`, `Constituencies`, `Subcounties`, `Parishes` |
| Blazor Search `/search` | `Search` + `AdministrativeLevel` filter + `maxResults` |
| Blazor Full code `/full-code` | `GetByFullCode` |
| Blazor Cascade `/cascade` | `Districts`, `GetDistrict`, child lists, `FullCode` binding, clear-on-parent-change |

```powershell
dotnet run --project samples\Uganda.AdministrativeUnits.Sample
dotnet run --project samples\Uganda.AdministrativeUnits.Blazor
```

---

## Quick reference — public surface

| Type | Members |
|---|---|
| `IAdministrativeUnitDirectory` | `Dataset`, `Statistics`, `Districts`, `Constituencies`, `Subcounties`, `Parishes`, `Villages`, `GetDistrict`, `GetDistrictByName`, `GetByFullCode`, `Search` |
| `AdministrativeUnitDirectory` | `Default`, `Load()`, + interface members |
| `AdministrativeUnit` | `Level`, `Code`, `FullCode`, `Name`, `Parent`, `Breadcrumb`, `Ancestors()`, `Equals`, `GetHashCode`, `ToString` |
| `District` | `Constituencies`, `Subcounties`, `Parishes`, `Villages` |
| `Constituency` | `District`, `Subcounties`, `Parishes`, `Villages` |
| `Subcounty` | `Constituency`, `District`, `Parishes`, `Villages` |
| `Parish` | `Subcounty`, `Constituency`, `District`, `Villages` |
| `Village` | `Parish`, `Subcounty`, `Constituency`, `District` |
| `DatasetInfo` | `Title`, `Edition`, `PublishedOn`, `SourceNote`, `CorrectionsApplied` |
| `DatasetStatistics` | `Districts`, `Constituencies`, `Subcounties`, `Parishes`, `Villages` |
| `AdministrativeLevel` | `District`…`Village` |
| `AddUgandaAdministrativeUnits` | DI registration |

Internal helpers (`NameNormalizer`, `DatasetLoader`, heaps, etc.) are not part of the supported API.

---

## Related docs

- [DATA.md](DATA.md) — provenance, validation, source quirks  
- [DEVELOPMENT.md](DEVELOPMENT.md) — local workspace and how to run samples  
- [README.md](../README.md) — install overview and design notes  
