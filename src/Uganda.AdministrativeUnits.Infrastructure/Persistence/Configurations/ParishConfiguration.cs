using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainParish = Uganda.AdministrativeUnits.Domain.Entities.Parish;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Configurations;

public sealed class ParishConfiguration : IEntityTypeConfiguration<DomainParish>
{
    public void Configure(EntityTypeBuilder<DomainParish> builder)
    {
        builder.ToTable("Parishes");
        builder.HasKey(x => x.FullCode);

        builder.Property(x => x.FullCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Breadcrumb).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.SubcountyFullCode).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.SubcountyFullCode);
        builder.HasIndex(x => x.Name);

        builder.HasMany(x => x.Villages)
            .WithOne(x => x.Parish!)
            .HasForeignKey(x => x.ParishFullCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
