using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <c>dotnet ef</c> migrations.
/// </summary>
public sealed class AdministrativeUnitsDbContextFactory : IDesignTimeDbContextFactory<AdministrativeUnitsDbContext>
{
    public AdministrativeUnitsDbContext CreateDbContext(string[] args)
    {
        string apiProjectPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Uganda.AdministrativeUnits.Api"));

        if (!Directory.Exists(apiProjectPath))
        {
            apiProjectPath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "src", "Uganda.AdministrativeUnits.Api"));
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration.GetConnectionString(
            DependencyInjection.ServiceCollectionExtensions.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{DependencyInjection.ServiceCollectionExtensions.ConnectionStringName}' was not found for design-time.");
        }

        DbContextOptionsBuilder<AdministrativeUnitsDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(connectionString);

        return new AdministrativeUnitsDbContext(optionsBuilder.Options);
    }
}
