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

/// <summary>Parish children (villages).</summary>
[ApiController]
[Authorize]
[Route("api/v1/parishes")]
[Produces("application/json")]
[Tags("Parishes")]
public sealed class ParishesController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Lists villages in a parish (paged, or all when <paramref name="getAll"/> is true).</summary>
    /// <remarks>
    /// For cascade dropdowns use <c>?getAll=true</c> or
    /// <c>GET /api/v1/cascade/{parishFullCode}/children</c>.
    /// </remarks>
    /// <param name="fullCode">Parish full code.</param>
    /// <param name="page">1-based page number. Ignored when <paramref name="getAll"/> is true.</param>
    /// <param name="pageSize">Page size (max 200). Ignored when <paramref name="getAll"/> is true.</param>
    /// <param name="getAll">When true, returns every village under the parish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Villages returned.</response>
    /// <response code="404">Parish not found.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("{fullCode}/villages")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UnitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVillages(
        string fullCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool getAll = false,
        CancellationToken cancellationToken = default) =>
        (await queryService
            .GetVillagesByParishFullCodeAsync(fullCode, page, pageSize, getAll, cancellationToken)
            .ConfigureAwait(false))
            .ToActionResult();
}
