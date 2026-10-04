using System.Collections.Generic;
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

/// <summary>Subcounty children (parishes).</summary>
[ApiController]
[Authorize]
[Route("api/v1/subcounties")]
[Produces("application/json")]
[Tags("Subcounties")]
public sealed class SubcountiesController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Lists parishes in a subcounty.</summary>
    /// <param name="fullCode">Subcounty full code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Parishes returned.</response>
    /// <response code="404">Subcounty not found.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("{fullCode}/parishes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UnitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetParishes(
        string fullCode,
        CancellationToken cancellationToken = default) =>
        (await queryService.GetParishesBySubcountyFullCodeAsync(fullCode, cancellationToken).ConfigureAwait(false)).ToActionResult();
}
