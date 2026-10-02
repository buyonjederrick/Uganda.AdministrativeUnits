namespace Uganda.AdministrativeUnits.Blazor;

/// <summary>Display model for an administrative unit (safe to bind in the UI).</summary>
public sealed record UnitSummary(
    string FullCode,
    string Name,
    string Breadcrumb,
    AdministrativeLevel Level)
{
    public static UnitSummary From(AdministrativeUnit unit) =>
        new(unit.FullCode, unit.Name, unit.Breadcrumb, unit.Level);
}
