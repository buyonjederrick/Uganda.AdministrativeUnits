using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Uganda.AdministrativeUnits.Api.Controllers;
using Uganda.AdministrativeUnits.Application.Abstractions;
using Uganda.AdministrativeUnits.Contracts.Dtos;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Api;

public sealed class MetaControllerTests
{
    [Fact]
    public async Task GetDataset_Should_ReturnActionResultFromService()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<DatasetInfoDto> envelope = ApiResponse.Ok(new DatasetInfoDto
        {
            Title = "title",
            Edition = "July 2022",
            PublishedOn = new System.DateOnly(2022, 7, 19),
            SourceNote = "note",
            CorrectionsApplied = 0,
        });
        service.Setup(x => x.GetDatasetInfoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(envelope);
        MetaController controller = new(service.Object);

        // Act
        IActionResult result = await controller.GetDataset();

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }

    [Fact]
    public async Task GetStatistics_Should_ReturnActionResultFromService()
    {
        // Arrange
        Mock<IAdministrativeUnitQueryService> service = new();
        ApiResponse<DatasetStatisticsDto> envelope = ApiResponse.Ok(new DatasetStatisticsDto
        {
            Districts = 1,
            Constituencies = 1,
            Subcounties = 1,
            Parishes = 1,
            Villages = 2,
        });
        service.Setup(x => x.GetStatisticsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(envelope);
        MetaController controller = new(service.Object);

        // Act
        IActionResult result = await controller.GetStatistics();

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeSameAs(envelope);
    }
}
