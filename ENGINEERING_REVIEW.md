# Engineering review and changes

## Scope inspected

Reviewed the WPF/MVVM solution layout, Core services, shared validation, database helper/configuration, SQL schema, and existing validation/smoke/page test projects from the supplied archive. The implementation already has a useful separation between `HotelKeeper.App`, `HotelKeeper.ViewModels`, and `HotelKeeper.Core`; service-level authorization; parameterized SQL; transactions; decimal money fields; domain rules; and reusable form validation. This change set preserves that architecture rather than introducing a new framework.

## Changes made

- Added `Input.Search` to normalize optional search text and enforce the shared 100-character search limit at service boundaries. Applied it to room, guest, audit, request, housekeeping task, maintenance report, reservation, and stay lists.
- Added service-layer allow-list validation for room/request/reservation status filters and reservation/housekeeping list scopes. Invalid scope values can no longer silently broaden a query to an unfiltered list.
- Replaced silent swallowing in the notification badge refresh with secure diagnostic logging while preserving the last known count. Cancellation remains an expected no-op.
- Added diagnostic logging when best-effort transaction rollback fails during disposal.
- Added validation tests for trimmed, blank, and overlong search input.
- Removed the supplied machine-local `appsettings.local.json` from the deliverable because it contained a root database username and a literal password. Added `appsettings.local.example.json` with a placeholder, and updated setup/test guidance to use environment configuration and a least-privilege test account.

## OOP and architecture assessment

- **Encapsulation:** domain and workflow services already control important room, reservation, and housekeeping transitions; entity state should continue to be mutated through service/domain operations rather than View code.
- **Abstraction:** current Core / ViewModels / WPF separation is retained. I did not add interfaces that have no meaningful alternate implementation or test seam.
- **Inheritance:** `ObservableObject` and `PageViewModel` provide shared presentation behavior; no additional base classes were introduced without a clear need.
- **Polymorphism:** the abstract page view-model base and command abstractions already support reusable behavior; no artificial subtype hierarchy was added.

## Verification status

- Source and project files were inspected from the supplied archive.
- Static review confirms the new validation calls are placed before list queries and use existing allow-list/domain validation helpers.
- **Build and tests were not run:** the execution environment does not have the .NET SDK (`dotnet` command is unavailable), and this is a WPF `net8.0-windows` solution. Run the commands in `README.md` on a Windows machine with the .NET 8 SDK and a disposable MySQL test database before treating this as verified production-ready.

## Remaining risks / follow-up

- Rotate the credentials found in the supplied local configuration if they were used by any real or shared database; removing a file from this clean deliverable does not revoke an exposed password or erase it from prior archives/history.
- The full integration tests require MySQL and may expose additional environment-specific failures.
- Review the existing database setup/migration behavior against a backup before deploying schema changes. No schema migration was added by this change set.

## MySQL driver package update

- Updated the `MySqlConnector` NuGet package reference from `2.3.7` to `2.6.2` in `src/HotelKeeper.Core/HotelKeeper.Core.csproj`. This updates the .NET database driver, **not** the installed MySQL Server version.
- Verify restore/build and database integration tests in a .NET 8 environment before deployment. This environment does not have the .NET SDK, so that verification has not been performed here.
