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

/// <summary>Constituency children (subcounties).</summary>
[ApiController]
[Authorize]
[Route("api/v1/constituencies")]
[Produces("application/json")]
[Tags("Constituencies")]
public sealed class ConstituenciesController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Lists subcounties in a constituency.</summary>
    /// <remarks>Use the nationwide <c>FullCode</c>, e.g. <c>06-028</c>.</remarks>
    /// <param name="fullCode">Constituency full code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Subcounties returned.</response>
    /// <response code="404">Constituency not found.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("{fullCode}/subcounties")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UnitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubcounties(
        string fullCode,
        CancellationToken cancellationToken = default) =>
        (await queryService.GetSubcountiesByConstituencyFullCodeAsync(fullCode, cancellationToken).ConfigureAwait(false)).ToActionResult();
}
