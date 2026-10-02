# Uganda.AdministrativeUnits

An offline, dependency-free .NET directory of Uganda's verified administrative units: **146 districts, 353 constituencies, 2,198 subcounties / town councils, 10,717 parishes and 71,230 villages**, with official codes, hierarchy navigation and name search.

- **Zero dependencies and no network calls.** The dataset (~490 KB compressed) is embedded in the assembly.
- **Immutable and thread-safe.** Parsed once on first use (~200 ms, ~24 MB), then every lookup is in-memory.
- **Navigable both ways.** `village.Parish.Subcounty.Constituency.District`, or `district.Villages`.
- **Nationally unique codes.** Source codes only repeat between siblings, so every unit also gets a `FullCode` such as `06-028-01-01-01`.
- **Verified.** The extraction is checked against the totals the source document prints about itself (see [docs/DATA.md](docs/DATA.md)).

Targets `netstandard2.0` (.NET Framework 4.6.1+, .NET Core 2.0+) and `net8.0`.

## Install

```
dotnet add package Uganda.AdministrativeUnits
```

On .NET Framework, NuGet resolves the `netstandard2.0` asset (requires **4.6.1** or newer).

## Use

```csharp
using System.Linq;
using Uganda.AdministrativeUnits;

IAdministrativeUnitDirectory directory = AdministrativeUnitDirectory.Default;

// Look up a district by code or by name (case, accents and extra spaces are ignored)
District hoima = directory.GetDistrict("06")!;            // or directory.GetDistrictByName("hoima")
Console.WriteLine($"{hoima.Name}: {hoima.Villages.Count()} villages");

// Walk down...
foreach (Constituency county in hoima.Constituencies)
    foreach (Subcounty subcounty in county.Subcounties)
        Console.WriteLine($"{county.Name} / {subcounty.Name}: {subcounty.Parishes.Count} parishes");

// ...or up
Village village = hoima.Villages.First();
Console.WriteLine(village.Breadcrumb);   // HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA › KASAMBYA I
Console.WriteLine(village.FullCode);     // 06-028-01-01-01
Console.WriteLine(village.District.Name);

// Resolve a stored code back to a unit
AdministrativeUnit? unit = directory.GetByFullCode("06-028-01-01-01");

// Search (ranked: exact, prefix, word-prefix, substring)
foreach (AdministrativeUnit hit in directory.Search("kyamuzizi", AdministrativeLevel.Village))
    Console.WriteLine(hit.Breadcrumb);
```

### Dependency injection

```csharp
builder.Services.AddUgandaAdministrativeUnits();   // registers IAdministrativeUnitDirectory as a singleton

public sealed class AddressService(IAdministrativeUnitDirectory directory) { /* ... */ }
```

## Design notes

| Topic | Decision |
|---|---|
| Hierarchy | `District → Constituency → Subcounty → Parish → Village`, exactly as the source register nests them. |
| Names | Kept verbatim from the source (upper case). Lookups and search fold case, Latin diacritics, apostrophe variants and whitespace. |
| Codes | `Code` is the source's local code. `FullCode` joins ancestor codes with `-` and is unique across the country: store this when you persist a reference to a unit. |
| Subcounty/town | The source lists subcounties, town councils, divisions and wards together; so does this package (`Subcounty`). |
| Serialization | The object graph has parent links, so don't serialize units directly; project to your own DTOs (e.g. `FullCode`, `Name`, `Breadcrumb`). |
| Search cost | O(n) scan with a bounded top-K heap: a few milliseconds even for one-letter queries. |

## Data

The data comes from *Uganda's Verified Administrative Units, July 2022* (generated 19 July 2022). Administrative units change over time; this package is a snapshot of that edition, not a live registry. See [docs/DATA.md](docs/DATA.md) for how it was extracted, how it was validated, and known source quirks.

> **Licensing:** the MIT licence covers this package's code. The underlying data belongs to its publisher; confirm you may redistribute it before publishing the package publicly.

## Sample apps

In this repository (see [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for the canonical workspace path):

```
dotnet run --project samples/Uganda.AdministrativeUnits.Sample
dotnet run --project samples/Uganda.AdministrativeUnits.Blazor
```

The Blazor sample includes overview, district browse, search, full-code lookup, and **cascade select** (`/cascade`).

## Repository layout

```
src/Uganda.AdministrativeUnits                    core library (no dependencies)
src/Uganda.AdministrativeUnits.DependencyInjection  AddUgandaAdministrativeUnits()
samples/Uganda.AdministrativeUnits.Sample         console demo
samples/Uganda.AdministrativeUnits.Blazor         Blazor Web App demo
tests/Uganda.AdministrativeUnits.Tests            xunit tests
tools/extract_units.py                            reproducible PDF → dataset extraction
docs/DEVELOPMENT.md                               where to open the repo locally
```

```
dotnet build -c Release
dotnet test
dotnet pack src/Uganda.AdministrativeUnits -c Release
```

## Regenerating the dataset

```
python tools/extract_units.py ADMINISTRATIVE_UNITS_IN_UGANDA_JULY_2022.pdf \
       src/Uganda.AdministrativeUnits/Data/uganda-administrative-units-2022-07.json.gz
```

Requires Python 3.8+ and `pdftotext` (poppler-utils). Output is byte-for-byte reproducible, and the script fails if its result disagrees with the document's own totals.
