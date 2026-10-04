using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Uganda.AdministrativeUnits.Api.Authentication;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    public IList<string> ApiKeys { get; set; } = new List<string>();
}
