# BuildLedger ERP

A Windows desktop ERP for a construction material supply business — customers, construction
sites, inventory, purchases, sales/delivery, customer ledger and payments, expenses, reports,
backup/restore, and user management. Runs fully offline on a local SQLite database.

## Tech stack
- .NET 10 (WPF, C#), MVVM (CommunityToolkit.Mvvm)
- EF Core + SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- Microsoft.Extensions.DependencyInjection / Hosting / Logging / Configuration
- FluentValidation, BCrypt.Net-Next
- xUnit tests

## Project layout
```
src/
  BuildLedger.Core/            domain entities, enums, exceptions
  BuildLedger.Application/     DTOs, service interfaces, common result types
  BuildLedger.Infrastructure/  EF Core DbContext + migrations, services, backup, security, seed data
  BuildLedger.App/             WPF UI — views, viewmodels, design system (Themes/), dialogs
  BuildLedger.Tests/           xUnit unit + integration tests
docs/                          implementation plan, progress log, schema, guides
```

## Running the app (development)
Requires the .NET 10 SDK.

```bash
dotnet build BuildLedger.slnx
dotnet run --project src/BuildLedger.App/BuildLedger.App.csproj
```

On first run the app creates its data folder at
`%LOCALAPPDATA%\BuildLedgerERP\` (database file `buildledger.db`, backups in `Backups\`),
applies EF Core migrations automatically, and seeds default reference data plus one admin
account:

- **Username:** `admin`
- **Password:** `admin123`

Change this password immediately after first login (Settings, or ask an Owner-role user).

## Running the tests
```bash
dotnet test src/BuildLedger.Tests/BuildLedger.Tests.csproj
```

## Building a release / installer
See [docs/RELEASE_NOTES.md](docs/RELEASE_NOTES.md) for the publish and packaging steps.

## Documentation
- [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md)
- [docs/PROGRESS.md](docs/PROGRESS.md)
- [docs/DATABASE_SCHEMA.md](docs/DATABASE_SCHEMA.md)
- [docs/USER_GUIDE.md](docs/USER_GUIDE.md)
- [docs/BACKUP_AND_RESTORE.md](docs/BACKUP_AND_RESTORE.md)
- [docs/RELEASE_NOTES.md](docs/RELEASE_NOTES.md)
