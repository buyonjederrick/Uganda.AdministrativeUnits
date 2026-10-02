namespace Uganda.AdministrativeUnits;

/// <summary>Number of units at each level of the hierarchy.</summary>
public sealed record DatasetStatistics(
    int Districts,
    int Constituencies,
    int Subcounties,
    int Parishes,
    int Villages);
