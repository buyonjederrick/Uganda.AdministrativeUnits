using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Uganda.AdministrativeUnits.Application.Services;
using Uganda.AdministrativeUnits.Infrastructure.Persistence;
using Uganda.AdministrativeUnits.Infrastructure.Persistence.Repositories;
using DomainConstituency = Uganda.AdministrativeUnits.Domain.Entities.Constituency;
using DomainDatasetMeta = Uganda.AdministrativeUnits.Domain.Entities.DatasetMeta;
using DomainDistrict = Uganda.AdministrativeUnits.Domain.Entities.District;
using DomainParish = Uganda.AdministrativeUnits.Domain.Entities.Parish;
using DomainSubcounty = Uganda.AdministrativeUnits.Domain.Entities.Subcounty;
using DomainVillage = Uganda.AdministrativeUnits.Domain.Entities.Village;

namespace Uganda.AdministrativeUnits.Api.Tests.Support;

public sealed class SqliteTestFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<AdministrativeUnitsDbContext> options = new DbContextOptionsBuilder<AdministrativeUnitsDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new AdministrativeUnitsDbContext(options);
        DbContext.Database.EnsureCreated();

        Repository = new DbRepository(DbContext);
        Service = new AdministrativeUnitQueryService(Repository, NullLogger<AdministrativeUnitQueryService>.Instance);
    }

    public AdministrativeUnitsDbContext DbContext { get; }

    public DbRepository Repository { get; }

    public AdministrativeUnitQueryService Service { get; }

    public async Task SeedHierarchyAsync()
    {
        DbContext.Districts.Add(new DomainDistrict
        {
            Code = "06",
            FullCode = "06",
            Name = "HOIMA",
            Breadcrumb = "HOIMA",
        });

        DbContext.Constituencies.Add(new DomainConstituency
        {
            Code = "028",
            FullCode = "06-028",
            Name = "BUGAHYA COUNTY",
            Breadcrumb = "HOIMA › BUGAHYA COUNTY",
            DistrictCode = "06",
        });

        DbContext.Subcounties.Add(new DomainSubcounty
        {
            Code = "01",
            FullCode = "06-028-01",
            Name = "BUHANIKA",
            Breadcrumb = "HOIMA › BUGAHYA COUNTY › BUHANIKA",
            ConstituencyFullCode = "06-028",
        });

        DbContext.Parishes.Add(new DomainParish
        {
            Code = "01",
            FullCode = "06-028-01-01",
            Name = "KATEREIGA",
            Breadcrumb = "HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA",
            SubcountyFullCode = "06-028-01",
        });

        DbContext.Villages.AddRange(
            new DomainVillage
            {
                Code = "01",
                FullCode = "06-028-01-01-01",
                Name = "KASAMBYA I",
                Breadcrumb = "HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA › KASAMBYA I",
                ParishFullCode = "06-028-01-01",
            },
            new DomainVillage
            {
                Code = "02",
                FullCode = "06-028-01-01-02",
                Name = "KASAMBYA II",
                Breadcrumb = "HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA › KASAMBYA II",
                ParishFullCode = "06-028-01-01",
            });

        DbContext.DatasetMeta.Add(new DomainDatasetMeta
        {
            Id = 1,
            Title = "Uganda's Verified Administrative Units",
            Edition = "July 2022",
            PublishedOn = new DateOnly(2022, 7, 19),
            SourceNote = "Test seed",
            CorrectionsApplied = 1,
        });

        await DbContext.SaveChangesAsync().ConfigureAwait(false);
        DbContext.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        DbContext.Dispose();
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
