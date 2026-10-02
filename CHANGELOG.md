# Changelog

## 1.1.0

- Multi-target `netstandard2.0` and `net8.0`, so the packages work on **.NET Framework 4.6.1+** as well as modern .NET.
- No public API changes; `DateOnly` and the rest of the surface stay the same on every TFM.

## 1.0.0

- Initial release with the July 2022 edition of Uganda's verified administrative units:
  146 districts, 353 constituencies, 2,198 subcounties/town councils, 10,717 parishes, 71,230 villages.
- `IAdministrativeUnitDirectory` with code/name lookup, `FullCode` resolution and ranked name search.
- `Uganda.AdministrativeUnits.DependencyInjection` with `AddUgandaAdministrativeUnits()`.
