using System.Collections.Generic;

namespace Uganda.AdministrativeUnits.Domain.Entities;

public sealed class Parish
{
    public string FullCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Breadcrumb { get; set; } = string.Empty;

    public string SubcountyFullCode { get; set; } = string.Empty;

    public Subcounty? Subcounty { get; set; }

    public ICollection<Village> Villages { get; set; } = new List<Village>();
}
