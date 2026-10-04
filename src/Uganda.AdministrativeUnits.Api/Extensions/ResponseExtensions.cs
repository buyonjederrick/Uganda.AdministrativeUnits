using Microsoft.AspNetCore.Mvc;
using Uganda.AdministrativeUnits.Contracts.Responses;

namespace Uganda.AdministrativeUnits.Api.Extensions;

public static class ResponseExtensions
{
    /// <summary>
    /// Turns a service response into its HTTP result, taking the status code from the envelope.
    /// </summary>
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response) =>
        new ObjectResult(response)
        {
            StatusCode = response.StatusCode,
        };
}
