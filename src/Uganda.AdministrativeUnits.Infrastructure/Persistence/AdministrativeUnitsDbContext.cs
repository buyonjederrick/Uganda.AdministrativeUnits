using Microsoft.EntityFrameworkCore;
using DomainConstituency = Uganda.AdministrativeUnits.Domain.Entities.Constituency;
using DomainDatasetMeta = Uganda.AdministrativeUnits.Domain.Entities.DatasetMeta;
using DomainDistrict = Uganda.AdministrativeUnits.Domain.Entities.District;
using DomainParish = Uganda.AdministrativeUnits.Domain.Entities.Parish;
using DomainSubcounty = Uganda.AdministrativeUnits.Domain.Entities.Subcounty;
using DomainVillage = Uganda.AdministrativeUnits.Domain.Entities.Village;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence;

public sealed class AdministrativeUnitsDbContext : DbContext
{
    public AdministrativeUnitsDbContext(DbContextOptions<AdministrativeUnitsDbContext> options)
        : base(options)
    {
    }

    public DbSet<DomainDistrict> Districts => Set<DomainDistrict>();

    public DbSet<DomainConstituency> Constituencies => Set<DomainConstituency>();

    public DbSet<DomainSubcounty> Subcounties => Set<DomainSubcounty>();

    public DbSet<DomainParish> Parishes => Set<DomainParish>();

    public DbSet<DomainVillage> Villages => Set<DomainVillage>();

    public DbSet<DomainDatasetMeta> DatasetMeta => Set<DomainDatasetMeta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdministrativeUnitsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
