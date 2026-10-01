# SupportDesk

A full-stack support ticketing tool — ASP.NET Core Web API backend, Angular frontend.

**Status:** backend complete; frontend scaffolded, pages not built yet; no automated tests yet.

See `ARCHITECTURE.md` for design rationale and decisions, if present.

## Stack

- **Backend:** ASP.NET Core Web API, .NET 9, controller-based (not minimal APIs)
- **ORM:** EF Core 9, SQL Server (LocalDB)
- **Frontend:** Angular — not started yet

## Running the backend

Prerequisites: .NET 9 SDK, SQL Server Express LocalDB (ships with Visual Studio; otherwise install separately), `dotnet-ef` global tool (`dotnet tool install --global dotnet-ef` if you don't have it).

From `SupportDesk.Api/`:

```
dotnet restore
dotnet ef database update
dotnet run
```

`dotnet ef database update` creates the `SupportDeskDb` LocalDB database and applies all migrations. The connection string lives in `appsettings.json` (`ConnectionStrings:DefaultConnection`), pointed at `(localdb)\mssqllocaldb` — no manual DB server setup needed on Windows.

The API listens on `http://localhost:5105` (see `Properties/launchSettings.json` for the full profile, including the HTTPS port). Seed data (5 agents, 20 tickets across all statuses/priorities, several deliberately overdue) is inserted automatically on first run — see `Data/DbSeeder.cs`. It's idempotent (checks whether `Agents` already has rows), so restarting the app won't duplicate rows.

To exercise the API without a full frontend, see `SupportDesk.Api.http` (works with the VS Code "REST Client" extension or natively in Visual Studio 2022) — it has example requests already in it.

