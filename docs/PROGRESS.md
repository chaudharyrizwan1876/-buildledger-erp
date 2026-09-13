# BuildLedger ERP — Progress Log

## 2026-09-13
- Inspected target folder (empty), confirmed .NET 10.0.300 SDK and git available.
- Initialized git repo, created solution `BuildLedger.sln`.
- Scaffolded 5 projects: Core, Application, Infrastructure, App (WPF), Tests (xUnit).
- Wired project references (App -> Infra/App/Core, Infra -> Core/Application, Application -> Core).
- Added NuGet packages: CommunityToolkit.Mvvm, FluentValidation, EFCore.Sqlite, EFCore.Design,
  Microsoft.Extensions.{Hosting,DependencyInjection,Configuration,Logging}, BCrypt.Net-Next.
- Installed dotnet-ef global tool.
- Wrote docs/IMPLEMENTATION_PLAN.md.
- Next: domain entities + DbContext + design system + shell.

### Status: SCAFFOLDING COMPLETE — beginning domain layer
