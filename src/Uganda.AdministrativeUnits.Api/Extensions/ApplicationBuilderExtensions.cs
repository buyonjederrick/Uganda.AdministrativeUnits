using Microsoft.AspNetCore.Builder;
using Uganda.AdministrativeUnits.Api.Middleware;

namespace Uganda.AdministrativeUnits.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Registers the GovPay-style domain-exception middleware that writes <c>ApiResponse</c> envelopes.
    /// </summary>
    public static IApplicationBuilder UseBadRequestExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<BadRequestExceptionMiddleware>();
}
