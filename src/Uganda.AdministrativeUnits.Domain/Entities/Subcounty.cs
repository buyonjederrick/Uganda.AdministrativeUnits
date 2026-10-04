using System.Collections.Generic;

namespace Uganda.AdministrativeUnits.Domain.Entities;

public sealed class Subcounty
{
    public string FullCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Breadcrumb { get; set; } = string.Empty;

    public string ConstituencyFullCode { get; set; } = string.Empty;

    public Constituency? Constituency { get; set; }

    public ICollection<Parish> Parishes { get; set; } = new List<Parish>();
}
