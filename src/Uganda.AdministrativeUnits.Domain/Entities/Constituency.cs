using System.Collections.Generic;

namespace Uganda.AdministrativeUnits.Domain.Entities;

public sealed class Constituency
{
    public string FullCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Breadcrumb { get; set; } = string.Empty;

    public string DistrictCode { get; set; } = string.Empty;

    public District? District { get; set; }

    public ICollection<Subcounty> Subcounties { get; set; } = new List<Subcounty>();
}
