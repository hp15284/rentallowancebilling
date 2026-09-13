# Rent Allowance Billing System

A digital replacement for the paper "ભાડા ભથ્થા બીલ" (Rent/Travel Allowance Bill)
form. Staff submit their travel allowance bills online instead of filling the
paper form, and the bill moves through the same approval chain the paper
form used:

```
Employee  -->  Branch Manager  -->  Accountant  -->  Approving Authority
(submits)      (recommends)        (verifies)        (AGM / Chief Executive
                                                        sanctions payment)
```

Built with **ASP.NET Core MVC (.NET 8)** and **Entity Framework Core** against
**SQL Server** (tested against SQL Server 2019 / 15.0.2000.5, and works with
SQL Server LocalDB for local development).

## Project layout

```
RentAllowanceBilling.sln
src/
  RentAllowanceBilling.Web/        ASP.NET Core MVC application
    Controllers/                   Account, Bills, Branches, Employees, Users
    Models/                        Domain entities + view models
    Data/                          EF Core DbContext, migrations, seed data
    Views/
```

## Domain model

| Entity | Purpose |
|---|---|
| `Branch` | Bank/company branch master |
| `Employee` | Staff member (name, designation, basic salary, branch) |
| `RentAllowanceBill` | One bill header: period, purpose, status, and the sign-off trail (branch manager, accountant, approving authority) |
| `RentAllowanceBillTrip` | One row of the paper form's travel table (date, from, to, times, fare, allowance) |
| `ApplicationUser` | ASP.NET Identity user, optionally linked to an `Employee` and a `Branch` |

Roles: `Admin`, `Employee`, `BranchManager`, `Accountant`, `ApprovingAuthority`.

## Prerequisites (Visual Studio 2022)

1. **Visual Studio 2022** (17.8+) with the **ASP.NET and web development**
   workload installed.
2. **.NET 8 SDK** (installed automatically by the VS2022 installer if the
   workload above is selected; otherwise download from
   https://dotnet.microsoft.com/download/dotnet/8.0).
3. **SQL Server** — either:
   - SQL Server Developer/Standard **15.0.2000.5** (SQL Server 2019) installed
     locally or on a reachable server, or
   - **SQL Server LocalDB** (installed with Visual Studio's "Data storage and
     processing" workload component) for zero-install local development.
4. **SQL Server Management Studio (SSMS)** (optional, for inspecting the
   database).

## Getting started

1. **Clone and open** `RentAllowanceBilling.sln` in Visual Studio 2022.

2. **Set the connection string** in
   `src/RentAllowanceBilling.Web/appsettings.json`:

   - For **LocalDB** (default, no changes needed):
     ```json
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RentAllowanceBillingDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     ```
   - For a **full SQL Server instance** (e.g. SQL Server 2019, 15.0.2000.5):
     ```json
     "DefaultConnection": "Server=YOUR_SERVER\\INSTANCE;Database=RentAllowanceBillingDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
     ```
     (or use `Trusted_Connection=True` for Windows Authentication.)

3. **Run the app** (F5, or `dotnet run` from
   `src/RentAllowanceBilling.Web`). On first startup the app automatically:
   - applies EF Core migrations (creates the database and all tables),
   - seeds the `Admin`, `Employee`, `BranchManager`, `Accountant`, and
     `ApprovingAuthority` roles,
   - seeds two sample branches (`Head Office`, `Branch 1`),
   - seeds a default administrator account:
     - **Email:** `admin@rentallowance.local`
     - **Password:** `Admin@12345`

   No manual `Update-Database` step is required, but if you prefer to manage
   migrations by hand you can still run, from
   `src/RentAllowanceBilling.Web`:
   ```
   dotnet ef database update
   ```
   (Package Manager Console equivalent: `Update-Database`.)

4. **Log in as Administrator** (`admin@rentallowance.local` /
   `Admin@12345`) and, under **Branches** / **Employees** / **Users**, set up
   your real branches, employee master records, and assign the
   `BranchManager`, `Accountant`, and `ApprovingAuthority` roles (plus a
   branch, for `BranchManager`) to the relevant staff accounts.

5. Staff register their own login from the **Register** page (creates an
   `Employee` record automatically), then submit bills from **My Bills → New
   Bill**.

## Bill workflow

1. **Employee** fills the bill (period, purpose, and one row per trip — date,
   from, to, departure/return time, fare, allowance) and saves it as a
   **Draft**, then **Submits** it (a bill number `RAB-yyyyMMdd-#####` is
   assigned).
2. **Branch Manager** (of the employee's branch) reviews it under *Pending
   Recommendation* and either **Recommends** it forward or **Rejects** it
   with a reason.
3. **Accountant** reviews it under *Pending Verification*, confirms/edits the
   amount, and **Verifies & Forwards** it (or rejects it).
4. **Approving Authority** (AGM / Chief Executive) gives the final
   **Approve** (with amount in words, mirroring the paper form's sanction
   line) or **Rejects** it.
5. Any bill can be opened as a **Print** view that mirrors the original
   paper form's layout and text, for record-keeping or physical filing.

## Notes on the SQL Server version

The connection strings above work unchanged against SQL Server
**15.0.2000.5** (SQL Server 2019 RTM) — EF Core's SQL Server provider targets
the SQL Server wire protocol/TDS version, not a specific build number, so no
provider changes are needed for that specific patch level. `TrustServerCertificate=True`
is included because SQL Server 2019+ defaults to requiring an encrypted
connection; if your server has a proper TLS certificate installed you can
remove that setting.

## Tests performed

This project was built and smoke-tested end-to-end in a Linux container
against a real SQL Server 2019 instance (Docker image
`mcr.microsoft.com/mssql/server:2019-latest`) prior to committing:
`dotnet build` (0 warnings/errors), migrations applied automatically on
startup, and the full bill lifecycle (register → create → submit → branch
manager recommend → accountant verify → approving authority approve →
print) was exercised via HTTP requests and confirmed working.
