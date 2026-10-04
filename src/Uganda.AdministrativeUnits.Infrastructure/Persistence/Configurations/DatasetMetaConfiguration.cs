using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainDatasetMeta = Uganda.AdministrativeUnits.Domain.Entities.DatasetMeta;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Configurations;

public sealed class DatasetMetaConfiguration : IEntityTypeConfiguration<DomainDatasetMeta>
{
    public void Configure(EntityTypeBuilder<DomainDatasetMeta> builder)
    {
        builder.ToTable("DatasetMeta");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Title).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Edition).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SourceNote).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.PublishedOn).IsRequired();
        builder.Property(x => x.CorrectionsApplied).IsRequired();
    }
}
