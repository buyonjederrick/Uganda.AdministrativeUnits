using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Uganda.AdministrativeUnits.Api.Controllers;
using Uganda.AdministrativeUnits.Application.Abstractions;
using Uganda.AdministrativeUnits.Contracts.Dtos;
using Uganda.AdministrativeUnits.Contracts.Responses;
using ContractsAdministrativeLevel = Uganda.AdministrativeUnits.Contracts.Enums.AdministrativeLevel;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Api;

public sealed class HierarchyControllersTests
{
    [Fact]
    public async Task ConstituenciesController_GetSubcounties_Should_ReturnServiceEnvelope()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<IReadOnlyList<UnitSummaryDto>> envelope = CreateListEnvelope("06-028-01", "BUHANIKA", ContractsAdministrativeLevel.Subcounty);
        service
            .Setup(x => x.GetSubcountiesByConstituencyFullCodeAsync("06-028", It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await new ConstituenciesController(service.Object).GetSubcounties("06-028");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }

    [Fact]
    public async Task SubcountiesController_GetParishes_Should_ReturnServiceEnvelope()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<IReadOnlyList<UnitSummaryDto>> envelope = CreateListEnvelope("06-028-01-01", "KATEREIGA", ContractsAdministrativeLevel.Parish);
        service
            .Setup(x => x.GetParishesBySubcountyFullCodeAsync("06-028-01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await new SubcountiesController(service.Object).GetParishes("06-028-01");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }

    [Fact]
    public async Task ParishesController_GetVillages_Should_ReturnServiceEnvelope()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<PagedResult<UnitSummaryDto>> envelope = ApiResponse.Ok(new PagedResult<UnitSummaryDto>
        {
            Items =
            [
                new UnitSummaryDto
                {
                    Code = "06-028-01-01-01",
                    Name = "KASAMBYA I",
                    Level = ContractsAdministrativeLevel.Village,
                    ParentCode = "06-028-01-01",
                    Breadcrumb = "path",
                },
            ],
            Page = 1,
            PageSize = 50,
            TotalCount = 1,
        });
        service
            .Setup(x => x.GetVillagesByParishFullCodeAsync("06-028-01-01", 1, 50, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await new ParishesController(service.Object).GetVillages("06-028-01-01");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }

    private static ApiResponse<IReadOnlyList<UnitSummaryDto>> CreateListEnvelope(
        string fullCode,
        string name,
        ContractsAdministrativeLevel level) =>
        ApiResponse.Ok<IReadOnlyList<UnitSummaryDto>>(
        [
            new UnitSummaryDto
            {
                Code = fullCode,
                Name = name,
                Level = level,
                ParentCode = "parent",
                Breadcrumb = name,
            },
        ]);
}
