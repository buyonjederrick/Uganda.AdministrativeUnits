using Microsoft.Extensions.DependencyInjection;
using Uganda.AdministrativeUnits.Application.Abstractions;
using Uganda.AdministrativeUnits.Application.Services;

namespace Uganda.AdministrativeUnits.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdministrativeUnitQueryService, AdministrativeUnitQueryService>();
        return services;
    }
}