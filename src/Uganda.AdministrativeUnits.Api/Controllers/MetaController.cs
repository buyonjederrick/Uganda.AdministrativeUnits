using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Uganda.AdministrativeUnits.Api.Extensions;
using Uganda.AdministrativeUnits.Application.Abstractions;
using Uganda.AdministrativeUnits.Contracts.Dtos;
using Uganda.AdministrativeUnits.Contracts.Responses;

namespace Uganda.AdministrativeUnits.Api.Controllers;

/// <summary>Dataset provenance and national counts.</summary>
[ApiController]
[Authorize]
[Route("api/v1/meta")]
[Produces("application/json")]
[Tags("Meta")]
public sealed class MetaController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Returns dataset provenance metadata.</summary>
    /// <response code="200">Metadata returned.</response>
    /// <response code="404">Dataset has not been seeded.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("dataset")]
    [ProducesResponseType(typeof(ApiResponse<DatasetInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDataset(CancellationToken cancellationToken = default) =>
        (await queryService.GetDatasetInfoAsync(cancellationToken).ConfigureAwait(false)).ToActionResult();

    /// <summary>Returns counts at each administrative level.</summary>
    /// <response code="200">Statistics returned.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<DatasetStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken = default) =>
        (await queryService.GetStatisticsAsync(cancellationToken).ConfigureAwait(false)).ToActionResult();
}