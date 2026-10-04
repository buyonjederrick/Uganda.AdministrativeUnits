# Development workspace

Use **this folder** as the only working copy of the repository:

```text
D:\Projects\Uganda.AdministrativeUnits\UgandaAdministrativeUnits
```

It is the git clone connected to `origin` (`buyonjederrick/Uganda.AdministrativeUnits`).

Do **not** edit the duplicate under `Downloads` (for example `C:\Users\Personal\Downloads\Uganda.AdministrativeUnits\...`). That copy is not tracked by git and changes there will not match what you run from `D:\Projects`.

## Open in Cursor or Visual Studio

- **Cursor / VS Code:** File → Open Folder → select the path above (or open `UgandaAdministrativeUnits.code-workspace` in the parent directory).
- **Visual Studio:** Open `Uganda.AdministrativeUnits.sln` in this folder.

## Sample apps

Console:

```powershell
dotnet run --project samples\Uganda.AdministrativeUnits.Sample\Uganda.AdministrativeUnits.Sample.csproj
```

Blazor (includes **Cascade select** at `/cascade`):

```powershell
dotnet run --project samples\Uganda.AdministrativeUnits.Blazor\Uganda.AdministrativeUnits.Blazor.csproj
```

Then browse to `https://localhost:7280` or `http://localhost:5280`.

| Route | What it shows |
|---|---|
| `/` | Dataset + statistics |
| `/districts` | `GetDistrict` / `GetDistrictByName` + hierarchy browse |
| `/search` | `Search` with level filter |
| `/full-code` | `GetByFullCode` |
| `/cascade` | District → village cascade select |

API usage for every method (including cascade): [API.md](API.md).

## SQL Web API

Production-style host: `src/Uganda.AdministrativeUnits.Api` (SQL Server source of truth after seed, API key auth, Scalar docs).

### 1. Configure

Edit `src/Uganda.AdministrativeUnits.Api/appsettings.Development.json` (or use user secrets):

- `ConnectionStrings:AdministrativeUnits` — SQL Server / LocalDB connection string
- `Authentication:ApiKeys` — one or more keys accepted via the `X-Api-Key` header

```powershell
dotnet user-secrets set "Authentication:ApiKeys:0" "your-production-key" --project src\Uganda.AdministrativeUnits.Api
```

### 2. Create the EF migration (manual)

Models and `AdministrativeUnitsDbContext` are already in place. **Create the migration yourself** (not committed by the AI workflow).

In **Development**, the API applies pending migrations on startup (`Database.MigrateAsync`) before seeding. You can still apply manually with `dotnet ef database update` if you prefer.

#### Option A — `dotnet ef` (recommended)

```powershell
dotnet tool install -g dotnet-ef
# or: dotnet tool update -g dotnet-ef

dotnet ef migrations add InitialCreate `
  --project src\Uganda.AdministrativeUnits.Infrastructure `
  --startup-project src\Uganda.AdministrativeUnits.Api `
  --output-dir Migrations

# Optional if you want to create the DB before the first API run:
dotnet ef database update `
  --project src\Uganda.AdministrativeUnits.Infrastructure `
  --startup-project src\Uganda.AdministrativeUnits.Api
```

#### Option B — Visual Studio Package Manager Console

1. Restore packages / rebuild the solution (so `Microsoft.EntityFrameworkCore.Tools` loads).
2. Restart Package Manager Console (or Visual Studio) if `Add-Migration` is still unknown.
3. Set **Default project** to `Uganda.AdministrativeUnits.Infrastructure`.
4. Run:

```powershell
Add-Migration InitialCreate -StartupProject Uganda.AdministrativeUnits.Api -OutputDir Migrations
Update-Database -StartupProject Uganda.AdministrativeUnits.Api
```

`add-migration` fails when the Tools package is not restored; use `Add-Migration` after a rebuild.

### 3. Run

```powershell
dotnet run --project src\Uganda.AdministrativeUnits.Api
```

Requires **.NET 10 SDK**, EF Core 10 packages, and **SQL Server LocalDB** (Development connection string uses `(localdb)\mssqllocaldb`). If LocalDB is missing, install the SQL Server Express LocalDB feature or point `ConnectionStrings:AdministrativeUnits` at an existing SQL Server instance.

Then open:

| URL | Purpose |
|---|---|
| `https://localhost:7281/scalar/v1` | Scalar API documentation |
| `https://localhost:7281/openapi/v1.json` | Built-in OpenAPI document (no Swagger UI) |
| `https://localhost:7281/health` | Health check (anonymous) |

### Cascade select (recommended)

Use unpaged cascade endpoints so dropdowns never miss options:

```http
GET /api/v1/cascade/districts
GET /api/v1/cascade/{code}/children
```

Example flow: `cascade/districts` → `cascade/06/children` → `cascade/06-028/children` → … → villages.

Also available:

| Endpoint | Notes |
|---|---|
| `GET /api/v1/districts?getAll=true` | All districts in a paged envelope |
| `GET /api/v1/parishes/{code}/villages?getAll=true` | All villages under a parish |
| `GET /api/v1/districts/{code}/constituencies` | Full child list |
| `GET /api/v1/constituencies/{code}/subcounties` | Full child list |
| `GET /api/v1/subcounties/{code}/parishes` | Full child list |
| `GET /api/v1/search?q=...&level=Village&parentCode=...` | Search any/all levels; optional parent scope |

In Scalar, authenticate with the `ApiKey` scheme / `X-Api-Key` header (Development default: `dev-api-key-change-me`).

Responses use `ApiResponse<T>` with `success`, `message`, `statusCode`, `data`, and `errors`. Docs are Scalar + ASP.NET Core OpenAPI only — Swashbuckle/Swagger UI is not used.

On first successful start against an empty database, the API seeds all districts → villages from the embedded library directory (idempotent; skipped if districts already exist). First seed can take a few minutes because of ~71k villages.

### Main endpoints

All `/api/v1/*` routes require `X-Api-Key`:

- `GET /api/v1/districts`
- `GET /api/v1/districts/{code}`
- `GET /api/v1/districts/{code}/constituencies`
- `GET /api/v1/constituencies/{fullCode}/subcounties`
- `GET /api/v1/subcounties/{fullCode}/parishes`
- `GET /api/v1/parishes/{fullCode}/villages`
- `GET /api/v1/units/{fullCode}`
- `GET /api/v1/search?q=&level=&maxResults=`
- `GET /api/v1/meta/dataset`
- `GET /api/v1/meta/statistics`

## Build and test

```powershell
dotnet build -c Release
dotnet test
dotnet test tests\Uganda.AdministrativeUnits.Api.Tests
```

- `Uganda.AdministrativeUnits.Tests` — core offline library
- `Uganda.AdministrativeUnits.Api.Tests` — Web API stack (contracts, application service, auth, middleware, controllers, EF/SQLite)
