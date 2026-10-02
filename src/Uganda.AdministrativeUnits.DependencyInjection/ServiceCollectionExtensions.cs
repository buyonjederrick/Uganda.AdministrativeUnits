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
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
#else
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
#endif

        services.TryAddSingleton<IAdministrativeUnitDirectory>(_ => AdministrativeUnitDirectory.Default);
        return services;
    }
}
