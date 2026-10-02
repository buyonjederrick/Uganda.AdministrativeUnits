using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Uganda.AdministrativeUnits.Internal;

internal sealed class DatasetDto
{
    public DatasetInfoDto Dataset { get; set; } = new();

    public List<DistrictDto> Districts { get; set; } = new();
}

internal sealed class DatasetInfoDto
{
    public string Title { get; set; } = string.Empty;

    public string Edition { get; set; } = string.Empty;

    public DateOnly PublishedOn { get; set; }

    public string SourceNote { get; set; } = string.Empty;

    public int CorrectionsApplied { get; set; }
}

internal sealed class DistrictDto : UnitDto
{
    public List<ConstituencyDto> Constituencies { get; set; } = new();
}

internal sealed class ConstituencyDto : UnitDto
{
    public List<SubcountyDto> Subcounties { get; set; } = new();
}

internal sealed class SubcountyDto : UnitDto
{
    public List<ParishDto> Parishes { get; set; } = new();
}

internal sealed class ParishDto : UnitDto
{
    public List<UnitDto> Villages { get; set; } = new();
}

internal class UnitDto
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DatasetDto))]
internal sealed partial class DatasetJsonContext : JsonSerializerContext
{
}
