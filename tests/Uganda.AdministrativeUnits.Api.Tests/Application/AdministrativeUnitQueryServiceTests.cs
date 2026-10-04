using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Uganda.AdministrativeUnits.Application.Exceptions;
using Uganda.AdministrativeUnits.Application.Services;
using Uganda.AdministrativeUnits.Contracts.Dtos;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Uganda.AdministrativeUnits.Api.Tests.Support;
using ContractsAdministrativeLevel = Uganda.AdministrativeUnits.Contracts.Enums.AdministrativeLevel;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Application;

public sealed class AdministrativeUnitQueryServiceTests
{
    [Fact]
    public async Task Constructor_Should_ThrowArgumentNullException_When_DependenciesAreNull()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Action missingDb = () => _ = new AdministrativeUnitQueryService(null!, NullLogger<AdministrativeUnitQueryService>.Instance);
        Action missingLogger = () => _ = new AdministrativeUnitQueryService(fixture.Repository, null!);

        // Assert
        missingDb.Should().Throw<ArgumentNullException>().WithParameterName("db");
        missingLogger.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task GetDistrictsAsync_Should_ReturnPagedDistricts_When_DataExists()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<PagedResult<DistrictDto>> response = await fixture.Service.GetDistrictsAsync(1, 50);

        // Assert
        response.Success.Should().BeTrue();
        response.Data!.TotalCount.Should().Be(1);
        response.Data.Items.Should().ContainSingle()
            .Which.Name.Should().Be("HOIMA");
        response.Data.Items[0].ConstituencyCount.Should().Be(1);
    }

    [Fact]
    public async Task GetDistrictByCodeAsync_Should_ReturnDistrict_When_CodeExists()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<DistrictDto> response = await fixture.Service.GetDistrictByCodeAsync(" 06 ");

        // Assert
        response.Success.Should().BeTrue();
        response.Data!.Code.Should().Be("06");
        response.Data.Name.Should().Be("HOIMA");
    }

    [Fact]
    public async Task GetDistrictByCodeAsync_Should_ThrowNotFoundException_When_CodeDoesNotExist()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        Func<Task> act = async () => await fixture.Service.GetDistrictByCodeAsync("99");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task GetDistrictByCodeAsync_Should_ThrowBadRequestException_When_CodeIsEmpty()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.GetDistrictByCodeAsync("   ");

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*required*");
    }

    [Fact]
    public async Task GetConstituenciesByDistrictCodeAsync_Should_ReturnChildren_When_DistrictExists()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response =
            await fixture.Service.GetConstituenciesByDistrictCodeAsync("06");

        // Assert
        response.Data.Should().ContainSingle()
            .Which.Code.Should().Be("06-028");
    }

    [Fact]
    public async Task GetConstituenciesByDistrictCodeAsync_Should_ThrowNotFoundException_When_DistrictMissing()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.GetConstituenciesByDistrictCodeAsync("06");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetSubcountiesByConstituencyFullCodeAsync_Should_ReturnChildren_When_ConstituencyExists()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response =
            await fixture.Service.GetSubcountiesByConstituencyFullCodeAsync("06-028");

        // Assert
        response.Data.Should().ContainSingle().Which.Name.Should().Be("BUHANIKA");
    }

    [Fact]
    public async Task GetSubcountiesByConstituencyFullCodeAsync_Should_ThrowNotFoundException_When_Missing()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.GetSubcountiesByConstituencyFullCodeAsync("06-999");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetParishesBySubcountyFullCodeAsync_Should_ReturnChildren_When_SubcountyExists()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response =
            await fixture.Service.GetParishesBySubcountyFullCodeAsync("06-028-01");

        // Assert
        response.Data.Should().ContainSingle().Which.Code.Should().Be("06-028-01-01");
    }

    [Fact]
    public async Task GetParishesBySubcountyFullCodeAsync_Should_ThrowNotFoundException_When_Missing()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.GetParishesBySubcountyFullCodeAsync("06-028-99");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetVillagesByParishFullCodeAsync_Should_ReturnPagedVillages_When_ParishExists()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<PagedResult<UnitSummaryDto>> response =
            await fixture.Service.GetVillagesByParishFullCodeAsync("06-028-01-01", page: 1, pageSize: 1);

        // Assert
        response.Data!.TotalCount.Should().Be(2);
        response.Data.Items.Should().ContainSingle();
        response.Data.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetVillagesByParishFullCodeAsync_Should_ReturnAllVillages_When_GetAllIsTrue()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<PagedResult<UnitSummaryDto>> response =
            await fixture.Service.GetVillagesByParishFullCodeAsync("06-028-01-01", page: 1, pageSize: 1, getAll: true);

        // Assert
        response.Data!.TotalCount.Should().Be(2);
        response.Data.Items.Should().HaveCount(2);
        response.Data.PageSize.Should().Be(2);
        response.Data.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetDistrictsAsync_Should_ReturnAllDistricts_When_GetAllIsTrue()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<PagedResult<DistrictDto>> response = await fixture.Service.GetDistrictsAsync(1, 1, getAll: true);

        // Assert
        response.Data!.Items.Should().ContainSingle().Which.Code.Should().Be("06");
        response.Data.TotalCount.Should().Be(1);
        response.Data.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetCascadeDistrictsAsync_Should_ReturnDistrictSummaries()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response = await fixture.Service.GetCascadeDistrictsAsync();

        // Assert
        response.Data.Should().ContainSingle()
            .Which.Level.Should().Be(ContractsAdministrativeLevel.District);
    }

    [Theory]
    [InlineData("06", ContractsAdministrativeLevel.Constituency, "06-028")]
    [InlineData("06-028", ContractsAdministrativeLevel.Subcounty, "06-028-01")]
    [InlineData("06-028-01", ContractsAdministrativeLevel.Parish, "06-028-01-01")]
    [InlineData("06-028-01-01", ContractsAdministrativeLevel.Village, "06-028-01-01-01")]
    public async Task GetChildrenByFullCodeAsync_Should_ReturnNextLevel_When_ParentExists(
        string parentFullCode,
        ContractsAdministrativeLevel expectedChildLevel,
        string expectedChildCode)
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response =
            await fixture.Service.GetChildrenByFullCodeAsync(parentFullCode);

        // Assert
        response.Data.Should().NotBeEmpty();
        response.Data.Should().OnlyContain(x => x.Level == expectedChildLevel);
        response.Data.Should().Contain(x => x.Code == expectedChildCode);
    }

    [Fact]
    public async Task GetChildrenByFullCodeAsync_Should_ReturnEmpty_When_ParentIsVillage()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response =
            await fixture.Service.GetChildrenByFullCodeAsync("06-028-01-01-01");

        // Assert
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVillagesByParishFullCodeAsync_Should_ThrowNotFoundException_When_ParishMissing()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.GetVillagesByParishFullCodeAsync("06-028-01-99", 1, 10);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData("06", ContractsAdministrativeLevel.District)]
    [InlineData("06-028", ContractsAdministrativeLevel.Constituency)]
    [InlineData("06-028-01", ContractsAdministrativeLevel.Subcounty)]
    [InlineData("06-028-01-01", ContractsAdministrativeLevel.Parish)]
    [InlineData("06-028-01-01-01", ContractsAdministrativeLevel.Village)]
    public async Task GetByFullCodeAsync_Should_ResolveCorrectLevel_When_FullCodeExists(
        string fullCode,
        ContractsAdministrativeLevel expectedLevel)
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<UnitSummaryDto> response = await fixture.Service.GetByFullCodeAsync(fullCode);

        // Assert
        response.Data!.Level.Should().Be(expectedLevel);
        response.Data.Code.Should().Be(fullCode);
    }

    [Fact]
    public async Task GetByFullCodeAsync_Should_ThrowNotFoundException_When_FullCodeIsUnknown()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        Func<Task> act = async () => await fixture.Service.GetByFullCodeAsync("06-028-01-01-99");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByFullCodeAsync_Should_ThrowNotFoundException_When_SegmentCountIsInvalid()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.GetByFullCodeAsync("1-2-3-4-5-6");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SearchAsync_Should_ReturnMatches_When_QueryMatchesName()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response =
            await fixture.Service.SearchAsync("HOIMA", level: null, maxResults: 10);

        // Assert
        response.Data.Should().Contain(x => x.Name == "HOIMA" && x.Level == ContractsAdministrativeLevel.District);
    }

    [Fact]
    public async Task SearchAsync_Should_RespectLevelFilter_When_LevelIsProvided()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response =
            await fixture.Service.SearchAsync("KASAMBYA", ContractsAdministrativeLevel.Village, maxResults: 10);

        // Assert
        response.Data.Should().OnlyContain(x => x.Level == ContractsAdministrativeLevel.Village);
        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_Should_ScopeToParentBranch_When_ParentCodeIsProvided()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<IReadOnlyList<UnitSummaryDto>> response = await fixture.Service.SearchAsync(
            "KASAMBYA",
            ContractsAdministrativeLevel.Village,
            maxResults: 10,
            parentFullCode: "06-028-01-01");

        // Assert
        response.Data.Should().HaveCount(2);
        response.Data.Should().OnlyContain(x => x.Code.StartsWith("06-028-01-01-", System.StringComparison.Ordinal));
    }

    [Fact]
    public async Task SearchAsync_Should_ThrowBadRequestException_When_QueryIsEmpty()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.SearchAsync("   ", null, 10);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*required*");
    }

    [Fact]
    public async Task GetDatasetInfoAsync_Should_ReturnMetadata_When_Seeded()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<DatasetInfoDto> response = await fixture.Service.GetDatasetInfoAsync();

        // Assert
        response.Data!.Edition.Should().Be("July 2022");
        response.Data.CorrectionsApplied.Should().Be(1);
    }

    [Fact]
    public async Task GetDatasetInfoAsync_Should_ThrowNotFoundException_When_NotSeeded()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();

        // Act
        Func<Task> act = async () => await fixture.Service.GetDatasetInfoAsync();

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*seeded*");
    }

    [Fact]
    public async Task GetStatisticsAsync_Should_ReturnCounts_When_DataExists()
    {
        // Arrange
        await using SqliteTestFixture fixture = new();
        await fixture.SeedHierarchyAsync();

        // Act
        ApiResponse<DatasetStatisticsDto> response = await fixture.Service.GetStatisticsAsync();

        // Assert
        response.Data!.Districts.Should().Be(1);
        response.Data.Constituencies.Should().Be(1);
        response.Data.Subcounties.Should().Be(1);
        response.Data.Parishes.Should().Be(1);
        response.Data.Villages.Should().Be(2);
    }
}
