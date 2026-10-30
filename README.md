## HotelKeeper

A WPF/MVVM hotel-management sample: front desk, housekeeping, maintenance, guest requests, payments, reporting and admin settings, backed by MySQL.

## Sample logins (first sign-in asks for a new password)
| Role | Username | Password |
|---|---|---|
| Admin | admin | Admin12345 |
| Receptionist | reception | Receptionist123 |
| Housekeeper | housekeeper | Housekeeper123 |
| Guest | can be registered | |

## Configuration and security

The checked-in `appsettings.json` contains only a placeholder connection string. Create `appsettings.local.json` from `appsettings.local.example.json` and put your local credentials there
(the local file is ignored by git), or set the `HOTELKEEPER_CONNECTION` environment variable, which takes precedence:

```json
{
  "ConnectionString": "Server=localhost;Port=3306;Database=hotelkeeper;User=hotelkeeper_app;Password=your-password;"
}
```

Use a dedicated MySQL account with only the privileges the application needs (SELECT/INSERT/UPDATE/DELETE on the
`hotelkeeper` database), not `root`. Never commit `appsettings.local.json`, log files, or build output; `.gitignore`
covers all three. Unexpected errors are written to
`%LOCALAPPDATA%\HotelKeeper\logs\hotelkeeper-YYYYMMDD.log` with connection-string passwords redacted, while users see
only safe, friendly messages.

## First run

Launch the application; `DatabaseSetup` creates the schema and starting data automatically when the database is empty.
For development sample data, set `HOTELKEEPER_SEED_DEV=1` before the first run.

## Tests

The tests expect a reachable MySQL server. Pass the same connection string used by the application:

```powershell
dotnet build HotelKeeper.sln -c Debug
dotnet run --project tests/ValidationTests/ValidationTests.csproj
dotnet run --project tests/Smoke/Smoke.csproj -- "$env:HOTELKEEPER_CONNECTION"
dotnet run --project tests/PageTests/PageTests.csproj -- "$env:HOTELKEEPER_CONNECTION"
```

`ValidationTests` is dependency-free and checks input rules, business rules, password handling, form/filter validation and
error sanitisation. `Smoke` drives real service workflows (including authorization, rollback, concurrency and failure
handling) against a disposable test database. `PageTests` drives the headless view models. Smoke and page tests generate unique guest
accounts on each run, so they can be run repeatedly. Use a dedicated least-privilege test account and never point these
mutation-heavy tests at production data.
