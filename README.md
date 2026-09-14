<div align="center">

# 🏗️ BuildLedger ERP

**Construction material supply, inventory & customer ledger management — built for the counter, not the boardroom.**

A native Windows desktop ERP for businesses selling steel rebar, cement, sand, bajri, bricks, blocks
and other construction supplies. Runs **100% offline** on a local SQLite database — no internet,
no subscription, no cloud dependency.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Desktop-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![SQLite](https://img.shields.io/badge/SQLite-EF%20Core-003B57?style=for-the-badge&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Tests](https://img.shields.io/badge/tests-26%20passing-2EA043?style=for-the-badge&logo=checkmarx&logoColor=white)]()
[![License](https://img.shields.io/badge/license-Proprietary-lightgrey?style=for-the-badge)]()

</div>

---

## ✨ Why BuildLedger ERP?

Most small construction-supply shops run their whole business on a paper *bahi khata* or a
scattered Excel sheet. BuildLedger ERP replaces that with one fast, offline desktop app — built
for a cashier to use all day, and for the owner to trust with the numbers.

| | |
|---|---|
| 🧾 **Smart customer search** | Type a name or phone number — matches existing customers instantly, blocks accidental duplicates |
| 🏗️ **Multi-site customers** | One contractor, many construction sites — ledger viewable per-site or consolidated |
| 📦 **Real inventory, real math** | Every purchase and sale writes an audited stock movement — stock never silently "adjusts" |
| 💰 **Correct accounting** | Sales = debit, payments = credit, advances show as negative balance — never a fake profit number |
| 🖨️ **Print-ready invoices & reports** | One click from Sales or Reports Center to the Windows print dialog |
| 🗄️ **Transaction-safe backups** | SQLite `VACUUM INTO` — a backup taken mid-sale is never corrupted |
| 🌓 **Light & dark themes** | A real design system, not default WPF gray |
| 🔒 **Role-based accounts** | Owner / Admin / Cashier, every action audit-logged |

---

## 📸 Screenshots

> _Drop your own screenshots into `docs/screenshots/` with these filenames and they'll render right here._

<div align="center">

| Dashboard | Sales & Delivery |
|---|---|
| ![Dashboard](docs/screenshots/dashboard.png) | ![Sales](docs/screenshots/sales.png) |

| Customer Ledger | Reports Center |
|---|---|
| ![Customers](docs/screenshots/customers.png) | ![Reports](docs/screenshots/reports.png) |

</div>

---

## 🧱 Tech Stack

<div align="center">

![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![.NET 10](https://img.shields.io/badge/.NET_10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-MVVM-0078D4?style=flat-square&logo=windows&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-Sqlite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-26_tests-25A162?style=flat-square)
![CommunityToolkit.Mvvm](https://img.shields.io/badge/CommunityToolkit.Mvvm-source_generators-purple?style=flat-square)

</div>

- **UI**: WPF + XAML, MVVM (`CommunityToolkit.Mvvm`), custom light/dark design system
- **Data**: EF Core + SQLite, code-first migrations, transactional stock/ledger posting
- **DI/Hosting**: `Microsoft.Extensions.Hosting` + `DependencyInjection`
- **Security**: BCrypt password hashing, role-based accounts, full audit log
- **Testing**: xUnit against a real in-memory SQLite database (no mocking the data layer)

---

## 🚀 Quick Start

```bash
# Clone
git clone https://github.com/<your-username>/buildledger-erp.git
cd buildledger-erp

# Build
dotnet build BuildLedger.slnx

# Run
dotnet run --project src/BuildLedger.App/BuildLedger.App.csproj
```

On first run the app creates `%LOCALAPPDATA%\BuildLedgerERP\`, applies migrations, and seeds a
default admin account:

```
Username: admin
Password: admin123
```

> ⚠️ Change this password immediately after first login.

### Run the tests

```bash
dotnet test src/BuildLedger.Tests/BuildLedger.Tests.csproj
```

### Build a release / installer

```powershell
./scripts/publish.ps1                 # self-contained win-x64 build → publish/
ISCC.exe scripts/installer.iss        # requires Inno Setup → dist/BuildLedgerERP-Setup-*.exe
```

---

## 📂 Project Structure

```
BuildLedger/
├── src/
│   ├── BuildLedger.Core/            # Domain entities, enums, exceptions
│   ├── BuildLedger.Application/     # DTOs, service interfaces, common result types
│   ├── BuildLedger.Infrastructure/  # EF Core, migrations, services, backup, security, seed data
│   ├── BuildLedger.App/             # WPF UI — views, viewmodels, design system, dialogs
│   └── BuildLedger.Tests/           # xUnit unit + integration tests
├── docs/                            # Implementation plan, schema, guides, progress log
├── scripts/                         # Publish + installer scripts
└── assets/
```

---

## 🗺️ Modules

`Dashboard` · `Customers & Sites` · `Products, Units & Categories` · `Suppliers` · `Purchases` ·
`Sales & Delivery` · `Payments & Receipts` · `Inventory & Stock Movements` · `Expenses` ·
`Reports Center` · `Backup & Restore` · `User Management` · `Settings` · `About`

## 📖 Documentation

| Doc | What's in it |
|---|---|
| [Implementation Plan](docs/IMPLEMENTATION_PLAN.md) | Architecture decisions, build phases |
| [Progress Log](docs/PROGRESS.md) | What's been built, in order, with rationale |
| [Database Schema](docs/DATABASE_SCHEMA.md) | Every table, relationship, and accounting rule |
| [User Guide](docs/USER_GUIDE.md) | How to use each screen day-to-day |
| [Backup & Restore](docs/BACKUP_AND_RESTORE.md) | How backups work, and why off-drive copies matter |
| [Release Notes](docs/RELEASE_NOTES.md) | What shipped, known limitations, what's next |

---

<div align="center">

Built with ❤️ for the people who actually run the yard, not just look at the spreadsheet.

</div>
