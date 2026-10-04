using System;

namespace Uganda.AdministrativeUnits.Domain.Entities;

public sealed class DatasetMeta
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Edition { get; set; } = string.Empty;

    public DateOnly PublishedOn { get; set; }

    public string SourceNote { get; set; } = string.Empty;

    public int CorrectionsApplied { get; set; }
}
