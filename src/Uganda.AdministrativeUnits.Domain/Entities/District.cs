using System.Collections.Generic;

namespace Uganda.AdministrativeUnits.Domain.Entities;

public sealed class District
{
    public string Code { get; set; } = string.Empty;

    public string FullCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Breadcrumb { get; set; } = string.Empty;

    public ICollection<Constituency> Constituencies { get; set; } = new List<Constituency>();
}
