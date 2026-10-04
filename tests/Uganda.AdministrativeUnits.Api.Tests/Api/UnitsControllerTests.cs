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

public sealed class UnitsControllerTests
{
    [Fact]
    public async Task GetByFullCode_Should_ReturnActionResultFromService_When_FullCodeIsValid()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<UnitSummaryDto> envelope = ApiResponse.Ok(new UnitSummaryDto
        {
            Code = "06-028-01-01-01",
            Name = "KASAMBYA I",
            Level = ContractsAdministrativeLevel.Village,
            ParentCode = "06-028-01-01",
            Breadcrumb = "HOIMA › BUGAHYA COUNTY › BUHANIKA › KATEREIGA › KASAMBYA I",
        });
        service
            .Setup(x => x.GetByFullCodeAsync("06-028-01-01-01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);
        UnitsController controller = new(service.Object);

        // Act
        IActionResult result = await controller.GetByFullCode("06-028-01-01-01");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }
}
