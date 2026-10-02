using System;
using System.Linq;
using Uganda.AdministrativeUnits;

IAdministrativeUnitDirectory directory = AdministrativeUnitDirectory.Default;

PrintDataset(directory);
PrintDistrictLookup(directory);
PrintHierarchyWalk(directory);
PrintFullCodeLookup(directory);
PrintSearch(directory);

static void PrintDataset(IAdministrativeUnitDirectory directory)
{
    DatasetInfo info = directory.Dataset;
    DatasetStatistics stats = directory.Statistics;

    Console.WriteLine("=== Dataset ===");
    Console.WriteLine($"{info.Title} ({info.Edition}, published {info.PublishedOn:yyyy-MM-dd})");
    Console.WriteLine(
        $"{stats.Districts} districts · {stats.Constituencies} constituencies · " +
        $"{stats.Subcounties} subcounties · {stats.Parishes} parishes · {stats.Villages} villages");
    Console.WriteLine();
}

static void PrintDistrictLookup(IAdministrativeUnitDirectory directory)
{
    Console.WriteLine("=== District lookup ===");

    District? byCode = directory.GetDistrict("06");
    District? byName = directory.GetDistrictByName("hoima");

    if (byCode is null || byName is null)
    {
        Console.WriteLine("Hoima district was not found.");
        Console.WriteLine();
        return;
    }

    Console.WriteLine($"By code \"06\": {byCode.Name} (FullCode {byCode.FullCode})");
    Console.WriteLine($"By name \"hoima\": {byName.Name} — {byName.Villages.Count()} villages");
    Console.WriteLine();
}

static void PrintHierarchyWalk(IAdministrativeUnitDirectory directory)
{
    Console.WriteLine("=== Hierarchy (Hoima) ===");

    District? hoima = directory.GetDistrict("06");
    if (hoima is null)
    {
        Console.WriteLine("Hoima district was not found.");
        Console.WriteLine();
        return;
    }

    foreach (Constituency county in hoima.Constituencies.Take(2))
    {
        foreach (Subcounty subcounty in county.Subcounties.Take(2))
        {
            Console.WriteLine($"{county.Name} / {subcounty.Name}: {subcounty.Parishes.Count} parishes");
        }
    }

    Village village = hoima.Villages.First();
    Console.WriteLine();
    Console.WriteLine($"First village: {village.Breadcrumb}");
    Console.WriteLine($"FullCode:      {village.FullCode}");
    Console.WriteLine($"District:      {village.District.Name}");
    Console.WriteLine();
}

static void PrintFullCodeLookup(IAdministrativeUnitDirectory directory)
{
    Console.WriteLine("=== FullCode lookup ===");

    const string fullCode = "06-028-01-01-01";
    AdministrativeUnit? unit = directory.GetByFullCode(fullCode);

    if (unit is null)
    {
        Console.WriteLine($"No unit for FullCode {fullCode}");
    }
    else
    {
        Console.WriteLine($"{fullCode} → {unit.Level}: {unit.Breadcrumb}");
    }

    Console.WriteLine();
}

static void PrintSearch(IAdministrativeUnitDirectory directory)
{
    Console.WriteLine("=== Search ===");

    Console.WriteLine("Query \"kampala\" (top 5):");
    foreach (AdministrativeUnit hit in directory.Search("kampala", maxResults: 5))
    {
        Console.WriteLine($"  [{hit.Level}] {hit.Breadcrumb} ({hit.FullCode})");
    }

    Console.WriteLine();
    Console.WriteLine("Villages matching \"kyamuzizi\":");
    foreach (AdministrativeUnit hit in directory.Search("kyamuzizi", AdministrativeLevel.Village))
    {
        Console.WriteLine($"  {hit.Breadcrumb}");
    }
}
