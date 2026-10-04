using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Uganda.AdministrativeUnits.Api.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static AuthenticationBuilder AddApiKeyAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        List<string> apiKeys = configuration.GetSection("Authentication:ApiKeys")
            .Get<List<string>>()?
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim())
            .Distinct(System.StringComparer.Ordinal)
            .ToList()
            ?? [];

        return services
            .AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                options =>
                {
                    options.ApiKeys = apiKeys;
                });
    }
}
