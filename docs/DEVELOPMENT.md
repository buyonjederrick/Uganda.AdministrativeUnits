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

## Build and test

```powershell
dotnet build -c Release
dotnet test
```
