using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainConstituency = Uganda.AdministrativeUnits.Domain.Entities.Constituency;
using DomainDistrict = Uganda.AdministrativeUnits.Domain.Entities.District;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Configurations;

public sealed class DistrictConfiguration : IEntityTypeConfiguration<DomainDistrict>
{
    public void Configure(EntityTypeBuilder<DomainDistrict> builder)
    {
        builder.ToTable("Districts");
        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).HasMaxLength(16).IsRequired();
        builder.Property(x => x.FullCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Breadcrumb).HasMaxLength(1024).IsRequired();

        builder.HasIndex(x => x.FullCode).IsUnique();
        builder.HasIndex(x => x.Name);

        builder.HasMany(x => x.Constituencies)
            .WithOne(x => x.District!)
            .HasForeignKey(x => x.DistrictCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
