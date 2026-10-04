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

/// <summary>District lookup and cascade children.</summary>
[ApiController]
[Authorize]
[Route("api/v1/districts")]
[Produces("application/json")]
[Tags("Districts")]
public sealed class DistrictsController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Lists districts with paging, or all districts when <paramref name="getAll"/> is true.</summary>
    /// <remarks>
    /// For cascade dropdowns prefer <c>GET /api/v1/cascade/districts</c> or <c>?getAll=true</c>
    /// so options are never truncated by the default page size.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/districts?getAll=true
    ///     X-Api-Key: your-api-key
    /// </remarks>
    /// <param name="page">1-based page number (default 1). Ignored when <paramref name="getAll"/> is true.</param>
    /// <param name="pageSize">Page size (default 50, max 200). Ignored when <paramref name="getAll"/> is true.</param>
    /// <param name="getAll">When true, returns every district in one response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Districts returned successfully.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DistrictDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDistricts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool getAll = false,
        CancellationToken cancellationToken = default) =>
        (await queryService.GetDistrictsAsync(page, pageSize, getAll, cancellationToken).ConfigureAwait(false))
            .ToActionResult();

    /// <summary>Gets a district by its local code.</summary>
    /// <remarks>
    /// Example: `GET /api/v1/districts/06` returns Hoima.
    /// </remarks>
    /// <param name="code">District local code, e.g. <c>06</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">District found.</response>
    /// <response code="400">Code was missing or empty.</response>
    /// <response code="404">District not found.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ApiResponse<DistrictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDistrictByCode(
        string code,
        CancellationToken cancellationToken = default) =>
        (await queryService.GetDistrictByCodeAsync(code, cancellationToken).ConfigureAwait(false)).ToActionResult();

    /// <summary>Lists all constituencies in a district (full list, no paging).</summary>
    /// <param name="code">District local code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Constituencies returned.</response>
    /// <response code="404">District not found.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("{code}/constituencies")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UnitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConstituencies(
        string code,
        CancellationToken cancellationToken = default) =>
        (await queryService.GetConstituenciesByDistrictCodeAsync(code, cancellationToken).ConfigureAwait(false))
            .ToActionResult();
}
