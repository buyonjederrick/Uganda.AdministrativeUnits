using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainSubcounty = Uganda.AdministrativeUnits.Domain.Entities.Subcounty;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Configurations;

public sealed class SubcountyConfiguration : IEntityTypeConfiguration<DomainSubcounty>
{
    public void Configure(EntityTypeBuilder<DomainSubcounty> builder)
    {
        builder.ToTable("Subcounties");
        builder.HasKey(x => x.FullCode);

        builder.Property(x => x.FullCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Breadcrumb).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.ConstituencyFullCode).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.ConstituencyFullCode);
        builder.HasIndex(x => x.Name);

        builder.HasMany(x => x.Parishes)
            .WithOne(x => x.Subcounty!)
            .HasForeignKey(x => x.SubcountyFullCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
