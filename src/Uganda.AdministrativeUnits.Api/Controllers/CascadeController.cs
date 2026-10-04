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

/// <summary>
/// Cascade-select helpers. Every response is a full child list (no paging),
/// so dropdowns never miss options.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/cascade")]
[Produces("application/json")]
[Tags("Cascade")]
public sealed class CascadeController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Lists every district for the root cascade dropdown.</summary>
    /// <remarks>
    /// Sample:
    ///
    ///     GET /api/v1/cascade/districts
    ///     X-Api-Key: your-api-key
    ///
    /// Then load children with <c>GET /api/v1/cascade/{fullCode}/children</c>.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">All districts returned.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("districts")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UnitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDistricts(CancellationToken cancellationToken = default) =>
        (await queryService.GetCascadeDistrictsAsync(cancellationToken).ConfigureAwait(false)).ToActionResult();

    /// <summary>Lists immediate children of a unit for the next cascade step.</summary>
    /// <remarks>
    /// Parent full-code segments:
    /// 1 → constituencies, 2 → subcounties, 3 → parishes, 4 → villages, 5 → empty list.
    ///
    /// Sample:
    ///
    ///     GET /api/v1/cascade/06/children
    ///     GET /api/v1/cascade/06-028/children
    ///     GET /api/v1/cascade/06-028-01/children
    ///     GET /api/v1/cascade/06-028-01-01/children
    /// </remarks>
    /// <param name="parentFullCode">Parent nationwide full code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Children returned (may be empty for a village).</response>
    /// <response code="400">Full code was missing or invalid.</response>
    /// <response code="404">Parent unit was not found.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("{parentFullCode}/children")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UnitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetChildren(
        string parentFullCode,
        CancellationToken cancellationToken = default) =>
        (await queryService.GetChildrenByFullCodeAsync(parentFullCode, cancellationToken).ConfigureAwait(false))
            .ToActionResult();
}
