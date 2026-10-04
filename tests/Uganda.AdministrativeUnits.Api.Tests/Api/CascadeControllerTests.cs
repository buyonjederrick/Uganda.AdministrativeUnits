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

public sealed class CascadeControllerTests
{
    [Fact]
    public async Task GetDistricts_Should_ReturnServiceEnvelope()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<IReadOnlyList<UnitSummaryDto>> envelope = ApiResponse.Ok<IReadOnlyList<UnitSummaryDto>>(
        [
            new UnitSummaryDto
            {
                Code = "06",
                Name = "HOIMA",
                Level = ContractsAdministrativeLevel.District,
                ParentCode = null,
                Breadcrumb = "HOIMA",
            },
        ]);
        service
            .Setup(x => x.GetCascadeDistrictsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await new CascadeController(service.Object).GetDistricts();

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }

    [Fact]
    public async Task GetChildren_Should_ReturnServiceEnvelope()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<IReadOnlyList<UnitSummaryDto>> envelope = ApiResponse.Ok<IReadOnlyList<UnitSummaryDto>>(
        [
            new UnitSummaryDto
            {
                Code = "06-028",
                Name = "BUGAHYA COUNTY",
                Level = ContractsAdministrativeLevel.Constituency,
                ParentCode = "06",
                Breadcrumb = "HOIMA › BUGAHYA COUNTY",
            },
        ]);
        service
            .Setup(x => x.GetChildrenByFullCodeAsync("06", It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await new CascadeController(service.Object).GetChildren("06");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }
}
