using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uganda.AdministrativeUnits.Application.Abstractions;
using DomainConstituency = Uganda.AdministrativeUnits.Domain.Entities.Constituency;
using DomainDatasetMeta = Uganda.AdministrativeUnits.Domain.Entities.DatasetMeta;
using DomainDistrict = Uganda.AdministrativeUnits.Domain.Entities.District;
using DomainParish = Uganda.AdministrativeUnits.Domain.Entities.Parish;
using DomainSubcounty = Uganda.AdministrativeUnits.Domain.Entities.Subcounty;
using DomainVillage = Uganda.AdministrativeUnits.Domain.Entities.Village;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Seeding;

public sealed partial class AdministrativeUnitsDataSeeder
{
    private const int BatchSize = 2000;

    private readonly IDbRepository _db;
    private readonly ILogger<AdministrativeUnitsDataSeeder> _logger;

    public AdministrativeUnitsDataSeeder(
        IDbRepository db,
        ILogger<AdministrativeUnitsDataSeeder> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        bool hasDistricts = await _db.GetDbSet<DomainDistrict>()
            .AsNoTracking()
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (hasDistricts)
        {
            LogAlreadySeeded(_logger);
            return;
        }

        LogSeedingStarted(_logger);

        AdministrativeUnitDirectory directory = AdministrativeUnitDirectory.Default;

        List<DomainDistrict> districts = directory.Districts
            .Select(d => new DomainDistrict
            {
                Code = d.Code,
                FullCode = d.FullCode,
                Name = d.Name,
                Breadcrumb = d.Breadcrumb,
            })
            .ToList();

        await InsertBatchesAsync(districts, cancellationToken).ConfigureAwait(false);
        LogSeededCount(_logger, "districts", districts.Count);

        List<DomainConstituency> constituencies = directory.Constituencies
            .Select(c => new DomainConstituency
            {
                FullCode = c.FullCode,
                Code = c.Code,
                Name = c.Name,
                Breadcrumb = c.Breadcrumb,
                DistrictCode = c.District.Code,
            })
            .ToList();

        await InsertBatchesAsync(constituencies, cancellationToken).ConfigureAwait(false);
        LogSeededCount(_logger, "constituencies", constituencies.Count);

        List<DomainSubcounty> subcounties = directory.Subcounties
            .Select(s => new DomainSubcounty
            {
                FullCode = s.FullCode,
                Code = s.Code,
                Name = s.Name,
                Breadcrumb = s.Breadcrumb,
                ConstituencyFullCode = s.Constituency.FullCode,
            })
            .ToList();

        await InsertBatchesAsync(subcounties, cancellationToken).ConfigureAwait(false);
        LogSeededCount(_logger, "subcounties", subcounties.Count);

        List<DomainParish> parishes = directory.Parishes
            .Select(p => new DomainParish
            {
                FullCode = p.FullCode,
                Code = p.Code,
                Name = p.Name,
                Breadcrumb = p.Breadcrumb,
                SubcountyFullCode = p.Subcounty.FullCode,
            })
            .ToList();

        await InsertBatchesAsync(parishes, cancellationToken).ConfigureAwait(false);
        LogSeededCount(_logger, "parishes", parishes.Count);

        List<DomainVillage> villages = directory.Villages
            .Select(v => new DomainVillage
            {
                FullCode = v.FullCode,
                Code = v.Code,
                Name = v.Name,
                Breadcrumb = v.Breadcrumb,
                ParishFullCode = v.Parish.FullCode,
            })
            .ToList();

        await InsertBatchesAsync(villages, cancellationToken).ConfigureAwait(false);
        LogSeededCount(_logger, "villages", villages.Count);

        DatasetInfo dataset = directory.Dataset;
        await _db.AddRangeAsync(
            [
                new DomainDatasetMeta
                {
                    Id = 1,
                    Title = dataset.Title,
                    Edition = dataset.Edition,
                    PublishedOn = dataset.PublishedOn,
                    SourceNote = dataset.SourceNote,
                    CorrectionsApplied = dataset.CorrectionsApplied,
                },
            ],
            cancellationToken).ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _db.ClearChangeTracker();
        LogSeedingCompleted(_logger);
    }

    private async Task InsertBatchesAsync<TEntity>(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken)
        where TEntity : class
    {
        for (int offset = 0; offset < entities.Count; offset += BatchSize)
        {
            int count = Math.Min(BatchSize, entities.Count - offset);
            List<TEntity> batch = entities.Skip(offset).Take(count).ToList();
            await _db.AddRangeAsync(batch, cancellationToken).ConfigureAwait(false);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _db.ClearChangeTracker();
        }
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Administrative units already seeded; skipping.")]
    private static partial void LogAlreadySeeded(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Seeding administrative units from embedded directory...")]
    private static partial void LogSeedingStarted(ILogger logger);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Seeded {Count} {EntityName}.")]
    private static partial void LogSeededCount(ILogger logger, string entityName, int count);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Administrative units seeding completed.")]
    private static partial void LogSeedingCompleted(ILogger logger);
}
