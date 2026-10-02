using System;

namespace Uganda.AdministrativeUnits;

/// <summary>Provenance of the data embedded in the package.</summary>
/// <param name="Title">Title of the source register.</param>
/// <param name="Edition">Edition of the register, e.g. "July 2022".</param>
/// <param name="PublishedOn">The date the source register was generated.</param>
/// <param name="SourceNote">Free-text note on how the data was obtained.</param>
/// <param name="CorrectionsApplied">Number of documented corrections applied to obvious source-data errors.</param>
public sealed record DatasetInfo(
    string Title,
    string Edition,
    DateOnly PublishedOn,
    string SourceNote,
    int CorrectionsApplied);
