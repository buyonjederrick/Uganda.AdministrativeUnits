using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uganda.AdministrativeUnits.Application.Abstractions;
using Uganda.AdministrativeUnits.Infrastructure.Persistence;
using Uganda.AdministrativeUnits.Infrastructure.Persistence.Repositories;
using Uganda.AdministrativeUnits.Infrastructure.Persistence.Seeding;

namespace Uganda.AdministrativeUnits.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public const string ConnectionStringName = "AdministrativeUnits";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured.");
        }

        services.AddDbContext<AdministrativeUnitsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.CommandTimeout(120);
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped<IDbRepository, DbRepository>();
        services.AddScoped<AdministrativeUnitsDataSeeder>();

        services.AddHealthChecks()
            .AddDbContextCheck<AdministrativeUnitsDbContext>("database");

        return services;
    }
}
