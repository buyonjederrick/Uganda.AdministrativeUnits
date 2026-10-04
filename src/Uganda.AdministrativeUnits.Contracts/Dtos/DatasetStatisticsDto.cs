namespace Uganda.AdministrativeUnits.Contracts.Dtos;

/// <summary>Counts at each administrative level.</summary>
public sealed class DatasetStatisticsDto
{
    public int Districts { get; set; }

    public int Constituencies { get; set; }

    public int Subcounties { get; set; }

    public int Parishes { get; set; }

    public int Villages { get; set; }
}
