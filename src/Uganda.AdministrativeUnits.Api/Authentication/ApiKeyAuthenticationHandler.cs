using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Uganda.AdministrativeUnits.Api.Authentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key header is missing."));
        }

        string? providedKey = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key header is empty."));
        }

        if (Options.ApiKeys.Count == 0)
        {
            return Task.FromResult(AuthenticateResult.Fail("No API keys are configured."));
        }

        bool isValid = Options.ApiKeys.Any(configured =>
            string.Equals(configured, providedKey, StringComparison.Ordinal));

        if (!isValid)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, "api-key-client"),
            new Claim(ClaimTypes.Name, "ApiKeyClient"),
        ];

        ClaimsIdentity identity = new(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
