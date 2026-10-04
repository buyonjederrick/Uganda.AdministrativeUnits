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

public sealed class DistrictsControllerTests
{
    private readonly Mock<IAdministrativeUnitQueryService> _service = new();
    private readonly DistrictsController _controller;

    public DistrictsControllerTests()
    {
        _controller = new DistrictsController(_service.Object);
    }

    [Fact]
    public async Task GetDistricts_Should_ReturnActionResultFromService_When_RequestIsValid()
    {
        // Arrange
        ApiResponse<PagedResult<DistrictDto>> envelope = ApiResponse.Ok(new PagedResult<DistrictDto>
        {
            Items = [new DistrictDto { Code = "06", Name = "HOIMA", ConstituencyCount = 1 }],
            Page = 1,
            PageSize = 50,
            TotalCount = 1,
        });
        _service
            .Setup(x => x.GetDistrictsAsync(1, 50, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await _controller.GetDistricts();

        // Assert
        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(200);
        objectResult.Value.Should().BeSameAs(envelope);
    }

    [Fact]
    public async Task GetDistrictByCode_Should_ReturnActionResultFromService_When_CodeIsValid()
    {
        // Arrange
        ApiResponse<DistrictDto> envelope = ApiResponse.Ok(new DistrictDto
        {
            Code = "06",
            Name = "HOIMA",
            ConstituencyCount = 1,
        });
        _service
            .Setup(x => x.GetDistrictByCodeAsync("06", It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await _controller.GetDistrictByCode("06");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }

    [Fact]
    public async Task GetConstituencies_Should_ReturnActionResultFromService_When_DistrictExists()
    {
        // Arrange
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
        _service
            .Setup(x => x.GetConstituenciesByDistrictCodeAsync("06", It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        // Act
        IActionResult result = await _controller.GetConstituencies("06");

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }
}
