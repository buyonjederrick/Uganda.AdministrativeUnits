using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uganda.AdministrativeUnits.Infrastructure.Persistence;
using Uganda.AdministrativeUnits.Infrastructure.Persistence.Seeding;

namespace Uganda.AdministrativeUnits.Api.Extensions;

internal static partial class StartupSeeding
{
    public static async Task SeedDatabaseAsync(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        AdministrativeUnitsDbContext dbContext = scope.ServiceProvider.GetRequiredService<AdministrativeUnitsDbContext>();
        AdministrativeUnitsDataSeeder seeder = scope.ServiceProvider.GetRequiredService<AdministrativeUnitsDataSeeder>();

        try
        {
            if (app.Environment.IsDevelopment())
            {
                await dbContext.Database.MigrateAsync().ConfigureAwait(false);
                LogMigrationsApplied(logger);
            }

            await seeder.SeedAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogSeedingFailed(logger, ex);
            throw;
        }
    }

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Error,
        Message = "Database startup failed. Check ConnectionStrings:AdministrativeUnits, ensure SQL Server/LocalDB is running, apply migrations, then restart.")]
    private static partial void LogSeedingFailed(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Information,
        Message = "Applied pending EF Core migrations (Development).")]
    private static partial void LogMigrationsApplied(ILogger logger);
}
