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

/// <summary>Resolve any administrative unit by nationwide FullCode.</summary>
[ApiController]
[Authorize]
[Route("api/v1/units")]
[Produces("application/json")]
[Tags("Units")]
public sealed class UnitsController(IAdministrativeUnitQueryService queryService) : ControllerBase
{
    /// <summary>Resolves any administrative unit by nationwide FullCode.</summary>
    /// <remarks>
    /// Example: `GET /api/v1/units/06-028-01-01-01`
    ///
    /// Segment count selects the table (1=district … 5=village) so lookup is a single indexed read.
    /// </remarks>
    /// <param name="fullCode">Nationwide full code, e.g. <c>06-028-01-01-01</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Unit found.</response>
    /// <response code="400">Full code was missing or empty.</response>
    /// <response code="404">Unit not found.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet("{fullCode}")]
    [ProducesResponseType(typeof(ApiResponse<UnitSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByFullCode(
        string fullCode,
        CancellationToken cancellationToken = default) =>
        (await queryService.GetByFullCodeAsync(fullCode, cancellationToken).ConfigureAwait(false)).ToActionResult();
}
