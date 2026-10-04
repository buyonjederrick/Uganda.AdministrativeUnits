using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainVillage = Uganda.AdministrativeUnits.Domain.Entities.Village;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Configurations;

public sealed class VillageConfiguration : IEntityTypeConfiguration<DomainVillage>
{
    public void Configure(EntityTypeBuilder<DomainVillage> builder)
    {
        builder.ToTable("Villages");
        builder.HasKey(x => x.FullCode);

        builder.Property(x => x.FullCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Breadcrumb).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.ParishFullCode).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.ParishFullCode);
        builder.HasIndex(x => x.Name);
    }
}
