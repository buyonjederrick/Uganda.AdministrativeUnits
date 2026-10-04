using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Uganda.AdministrativeUnits.Api.Authentication;

namespace Uganda.AdministrativeUnits.Api.OpenApi;

internal static class OpenApiConfiguration
{
    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(static (document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Uganda Administrative Units API",
                    Version = "v1",
                    Description =
                        """
                        Production read-only API for Uganda's verified administrative units
                        (District → Constituency → Subcounty → Parish → Village).

                        ## Authentication
                        All `/api/v1/*` endpoints require the `X-Api-Key` header.
                        Development default: `dev-api-key-change-me`.

                        ## Response envelope
                        Every JSON response uses `ApiResponse<T>`:
                        - `success` — whether the operation succeeded
                        - `message` — human-readable summary
                        - `statusCode` — HTTP status mirrored in the body
                        - `data` — payload (`null` on failure)
                        - `errors` — present only on failure

                        ### Success example
                        ```json
                        {
                          "success": true,
                          "message": "Success",
                          "statusCode": 200,
                          "data": {
                            "code": "06",
                            "name": "HOIMA",
                            "constituencyCount": 2
                          }
                        }
                        ```

                        ### Error example
                        ```json
                        {
                          "success": false,
                          "message": "District '99' was not found.",
                          "statusCode": 404,
                          "data": null,
                          "errors": [ "District '99' was not found." ]
                        }
                        ```

                        ## Cascade selects (recommended)
                        Use the unpaged cascade endpoints so dropdowns never miss options:
                        1. `GET /api/v1/cascade/districts`
                        2. `GET /api/v1/cascade/{code}/children` (repeat for each next level)

                        Equivalent hierarchy routes also return full child lists
                        (except villages/districts lists, which support `?getAll=true`).

                        ## Search
                        `GET /api/v1/search?q=...` searches all levels.
                        Filter with `level=District|Constituency|Subcounty|Parish|Village`
                        and optionally scope with `parentCode` for in-branch typeahead.

                        ## Hierarchy tips
                        Persist and query by `code` (nationwide unique path, e.g. `06-028-01-01-01`).
                        """,
                    Contact = new OpenApiContact
                    {
                        Name = "Uganda Administrative Units",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                    },
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes[ApiKeyAuthenticationOptions.DefaultScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = ApiKeyAuthenticationOptions.HeaderName,
                    Description = "API key required for all /api/v1 endpoints.",
                };

                document.Security ??= [];
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(ApiKeyAuthenticationOptions.DefaultScheme, document)] = [],
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
