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
using ContractsAdministrativeLevel = Uganda.AdministrativeUnits.Contracts.Enums.AdministrativeLevel;

namespace Uganda.AdministrativeUnits.Api.Controllers;

/// <summary>Name search across administrative units at any level.</summary>
[ApiController]
[Authorize]
[Route("api/v1/search")]
[Produces("application/json")]
[Tags("Search")]
public sealed class SearchController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Searches unit names across all levels, or a single level / parent branch.</summary>
    /// <remarks>
    /// Samples:
    ///
    ///     GET /api/v1/search?q=kalungu
    ///     GET /api/v1/search?q=kalungu&amp;level=Village
    ///     GET /api/v1/search?q=kasambya&amp;parentCode=06-028-01-01&amp;level=Village
    ///
    /// Level values: District, Constituency, Subcounty, Parish, Village.
    /// When <c>parentCode</c> is set, only descendants of that unit are searched.
    /// </remarks>
    /// <param name="query">Text to search for (required).</param>
    /// <param name="level">Optional level filter.</param>
    /// <param name="parentCode">Optional parent code to scope cascade typeahead.</param>
    /// <param name="maxResults">Maximum results (default 25, max 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Search results returned.</response>
    /// <response code="400">Query was missing or empty.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UnitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "q")] string query,
        [FromQuery] ContractsAdministrativeLevel? level = null,
        [FromQuery] string? parentCode = null,
        [FromQuery] int maxResults = 25,
        CancellationToken cancellationToken = default) =>
        (await queryService
            .SearchAsync(query, level, maxResults, parentCode, cancellationToken)
            .ConfigureAwait(false))
            .ToActionResult();
}
