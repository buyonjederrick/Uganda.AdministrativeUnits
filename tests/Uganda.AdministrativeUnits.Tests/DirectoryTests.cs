using System;
using System.Linq;
using Uganda.AdministrativeUnits;
using Xunit;

namespace Uganda.AdministrativeUnits.Tests;

public class DirectoryTests
{
    private static readonly AdministrativeUnitDirectory Directory = AdministrativeUnitDirectory.Default;
    private static readonly string[] HoimaVillageAncestorNames = ["KATEREIGA", "BUHANIKA", "BUGAHYA COUNTY", "HOIMA"];

    [Fact]
    public void Statistics_match_the_totals_printed_in_the_source_register()
    {
        DatasetStatistics s = Directory.Statistics;

        Assert.Equal(146, s.Districts);
        Assert.Equal(353, s.Constituencies);
        Assert.Equal(2198, s.Subcounties);
        Assert.Equal(10717, s.Parishes);
        Assert.Equal(71230, s.Villages);
        Assert.Equal(s.Villages, Directory.Villages.Count);
    }

    [Fact]
    public void Dataset_describes_its_provenance()
    {
        Assert.Equal("July 2022", Directory.Dataset.Edition);
        Assert.Equal(new DateOnly(2022, 7, 19), Directory.Dataset.PublishedOn);
    }

    [Fact]
    public void Default_is_a_shared_instance()
    {
        Assert.Same(AdministrativeUnitDirectory.Default, AdministrativeUnitDirectory.Default);
    }

    [Fact]
    public void Districts_can_be_found_by_code_and_by_name()
    {
        District? byCode = Directory.GetDistrict("06");
        District? byName = Directory.GetDistrictByName("  hoima ");

        Assert.NotNull(byCode);
        Assert.Equal("HOIMA", byCode.Name);
        Assert.Same(byCode, byName);
        Assert.Null(Directory.GetDistrict("000"));
        Assert.Null(Directory.GetDistrictByName("Atlantis"));
    }

    [Fact]
    public void Hierarchy_can_be_navigated_in_both_directions()
    {
        Village village = Directory.GetDistrict("06")!.Villages.First();

        Assert.Equal("06-028-01-01-01", village.FullCode);
        Assert.Equal("HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA › KASAMBYA I", village.Breadcrumb);
        Assert.Equal("HOIMA", village.District.Name);
        Assert.Equal("BUGAHYA COUNTY", village.Constituency.Name);
        Assert.Equal("BUHANIKA", village.Subcounty.Name);
        Assert.Equal("KATEREIGA", village.Parish.Name);
        Assert.Equal(HoimaVillageAncestorNames, village.Ancestors().Select(a => a.Name));
        Assert.Contains(village, village.Parish.Villages);
    }

    [Fact]
    public void Every_unit_has_a_unique_full_code_that_round_trips()
    {
        var all = Directory.Districts.Cast<AdministrativeUnit>()
            .Concat(Directory.Constituencies)
            .Concat(Directory.Subcounties)
            .Concat(Directory.Parishes)
            .Concat(Directory.Villages)
            .ToList();

        Assert.Equal(all.Count, all.Select(u => u.FullCode).Distinct().Count());
        Assert.All(all, unit => Assert.Same(unit, Directory.GetByFullCode(unit.FullCode)));
    }

    [Fact]
    public void Every_parent_lists_its_children_and_every_parish_has_villages()
    {
        Assert.All(Directory.Parishes, p => Assert.NotEmpty(p.Villages));
        Assert.All(Directory.Villages, v => Assert.Contains(v, v.Parish.Villages));
        Assert.All(Directory.Subcounties, s => Assert.Contains(s, s.Constituency.Subcounties));
    }

    [Fact]
    public void A_documented_source_correction_is_applied()
    {
        Village butta = Directory.Villages.Single(v => v.Name == "BUTTA" && v.Parish.Name == "BUNAMONE WARD");

        Assert.Equal("02", butta.Code);
        Assert.Equal(1, Directory.Dataset.CorrectionsApplied);
    }

    [Fact]
    public void Search_ranks_exact_matches_first_and_honours_the_level_filter()
    {
        var results = Directory.Search("kampala");

        Assert.Equal(AdministrativeLevel.District, results[0].Level);
        Assert.Equal("KAMPALA", results[0].Name);
        Assert.All(Directory.Search("kampala", AdministrativeLevel.Parish), r => Assert.Equal(AdministrativeLevel.Parish, r.Level));
        Assert.True(Directory.Search("a", maxResults: 10).Count == 10);
    }

    [Fact]
    public void Search_ignores_case_diacritics_and_extra_whitespace()
    {
        Assert.Contains(Directory.Search("  payila "), u => u.Name == "PAYÍLA");
        Assert.Empty(Directory.Search("   "));
    }

    [Fact]
    public void Search_validates_its_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => Directory.Search(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => Directory.Search("x", maxResults: 0));
    }
}
