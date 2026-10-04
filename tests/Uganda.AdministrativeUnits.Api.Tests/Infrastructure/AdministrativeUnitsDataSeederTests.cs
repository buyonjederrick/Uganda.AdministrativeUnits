using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Uganda.AdministrativeUnits.Api.Tests.Support;
using Uganda.AdministrativeUnits.Infrastructure.Persistence.Seeding;
using DomainDistrict = Uganda.AdministrativeUnits.Domain.Entities.District;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Infrastructure;

public sealed class AdministrativeUnitsDataSeederTests
{
    [Fact]
    public async Task Constructor_Should_ThrowArgumentNullException_When_DependenciesAreNull()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Action missingDb = () => _ = new AdministrativeUnitsDataSeeder(null!, NullLogger<AdministrativeUnitsDataSeeder>.Instance);
        Action missingLogger = () => _ = new AdministrativeUnitsDataSeeder(fixture.Repository, null!);

        // Assert
        missingDb.Should().Throw<ArgumentNullException>().WithParameterName("db");
        missingLogger.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task SeedAsync_Should_Skip_When_DistrictsAlreadyExist()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.Repository.AddRangeAsync(
        [
            new DomainDistrict
            {
                Code = "06",
                FullCode = "06",
                Name = "HOIMA",
                Breadcrumb = "HOIMA",
            },
        ]);
        await fixture.Repository.SaveChangesAsync();
        fixture.Repository.ClearChangeTracker();

        AdministrativeUnitsDataSeeder seeder = new(fixture.Repository, NullLogger<AdministrativeUnitsDataSeeder>.Instance);

        // Act
        await seeder.SeedAsync();

        // Assert
        (await fixture.DbContext.Constituencies.CountAsync()).Should().Be(0);
        (await fixture.DbContext.DatasetMeta.CountAsync()).Should().Be(0);
    }
}
