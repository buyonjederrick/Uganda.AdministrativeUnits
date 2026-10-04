using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainConstituency = Uganda.AdministrativeUnits.Domain.Entities.Constituency;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Configurations;

public sealed class ConstituencyConfiguration : IEntityTypeConfiguration<DomainConstituency>
{
    public void Configure(EntityTypeBuilder<DomainConstituency> builder)
    {
        builder.ToTable("Constituencies");
        builder.HasKey(x => x.FullCode);

        builder.Property(x => x.FullCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Breadcrumb).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.DistrictCode).HasMaxLength(16).IsRequired();

        builder.HasIndex(x => x.DistrictCode);
        builder.HasIndex(x => x.Name);

        builder.HasMany(x => x.Subcounties)
            .WithOne(x => x.Constituency!)
            .HasForeignKey(x => x.ConstituencyFullCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
