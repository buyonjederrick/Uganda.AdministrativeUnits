namespace Uganda.AdministrativeUnits.Domain.Entities;

public sealed class Village
{
    public string FullCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Breadcrumb { get; set; } = string.Empty;

    public string ParishFullCode { get; set; } = string.Empty;

    public Parish? Parish { get; set; }
}
