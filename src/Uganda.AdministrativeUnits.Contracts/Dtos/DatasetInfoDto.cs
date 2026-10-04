using System;

namespace Uganda.AdministrativeUnits.Contracts.Dtos;

/// <summary>Provenance of the persisted administrative units dataset.</summary>
public sealed class DatasetInfoDto
{
    public required string Title { get; init; }

    public required string Edition { get; init; }

    public required DateOnly PublishedOn { get; init; }

    public required string SourceNote { get; init; }

    public int CorrectionsApplied { get; init; }
}
