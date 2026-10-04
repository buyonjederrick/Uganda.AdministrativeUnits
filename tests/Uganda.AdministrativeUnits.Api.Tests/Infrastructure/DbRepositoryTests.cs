using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uganda.AdministrativeUnits.Api.Tests.Support;
using Uganda.AdministrativeUnits.Infrastructure.Persistence.Repositories;
using DomainDistrict = Uganda.AdministrativeUnits.Domain.Entities.District;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Infrastructure;

public sealed class DbRepositoryTests
{
    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_DbContextIsNull()
    {
        // Act
        Action act = () => _ = new DbRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    [Fact]
    public async Task AddRangeAsync_And_SaveChangesAsync_Should_PersistEntities()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        DomainDistrict district = new()
        {
            Code = "01",
            FullCode = "01",
            Name = "KAMPALA",
            Breadcrumb = "KAMPALA",
        };

        // Act
        await fixture.Repository.AddRangeAsync([district]);
        int saved = await fixture.Repository.SaveChangesAsync();
        fixture.Repository.ClearChangeTracker();

        // Assert
        saved.Should().Be(1);
        DomainDistrict? found = await fixture.Repository.GetDbSet<DomainDistrict>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "01");
        found.Should().NotBeNull();
        fixture.Repository.GetDbContext().Should().BeSameAs(fixture.DbContext);
    }
}
