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

public sealed class SearchControllerTests
{
    [Fact]
    public async Task Search_Should_ReturnActionResultFromService_When_QueryIsValid()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<IReadOnlyList<UnitSummaryDto>> envelope = ApiResponse.Ok<IReadOnlyList<UnitSummaryDto>>([]);
        service
            .Setup(x => x.SearchAsync(
                "hoima",
                ContractsAdministrativeLevel.District,
                10,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);
        SearchController controller = new(service.Object);

        // Act
        IActionResult result = await controller.Search("hoima", ContractsAdministrativeLevel.District, null, 10);

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }

    [Fact]
    public async Task Search_Should_PassParentCode_When_Provided()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<IReadOnlyList<UnitSummaryDto>> envelope = ApiResponse.Ok<IReadOnlyList<UnitSummaryDto>>([]);
        service
            .Setup(x => x.SearchAsync(
                "kasambya",
                ContractsAdministrativeLevel.Village,
                25,
                "06-028-01-01",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);
        SearchController controller = new(service.Object);

        // Act
        IActionResult result = await controller.Search(
            "kasambya",
            ContractsAdministrativeLevel.Village,
            "06-028-01-01");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
        service.VerifyAll();
    }
}
