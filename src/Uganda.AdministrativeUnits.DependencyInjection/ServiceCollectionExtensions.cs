using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Uganda.AdministrativeUnits;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Dependency-injection registration for <see cref="IAdministrativeUnitDirectory"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IAdministrativeUnitDirectory"/> as a singleton backed by the shared,
    /// lazily loaded <see cref="AdministrativeUnitDirectory.Default"/> instance. Safe to call more than once.
    /// </summary>
    public static IServiceCollection AddUgandaAdministrativeUnits(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IAdministrativeUnitDirectory>(static _ => AdministrativeUnitDirectory.Default);
        return services;
    }
}
