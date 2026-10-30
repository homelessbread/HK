# AGENT.md — Hotel Room Keeper
## Shared Development and Merge Coordination Instructions

This file contains the shared development rules for both the UI Agent and Backend Agent. Both agents MUST follow these instructions throughout implementation.

The objective is to allow independent UI and Backend development while ensuring both implementations remain compatible and can be merged into one working application.

---

# 1. PROJECT-WIDE RULES

1. Read this entire `AGENT.md` before making changes.
2. Inspect the existing project structure, architecture, dependencies, database, and requirements.
3. Preserve existing working features and design.
4. Do not rebuild working components unnecessarily.
5. Do not delete, rename, or restructure shared files without a valid reason.
6. Avoid unnecessary dependencies and duplicate implementations.
7. Follow the existing coding conventions and project architecture.
8. Keep the application buildable and runnable throughout development.
9. Do not assume that another agent has completed a feature unless its implementation has been verified.
10. Document all completed, unfinished, and blocked work in `UPDATES.md`.

# 2. AGENT RESPONSIBILITIES

Both agents must work independently within their assigned responsibilities.

## UI AGENT

**Primary responsibility:** UI, presentation, user interaction, layouts, components, navigation, and ViewModels.

Allowed responsibilities:
- UI layouts and styling.
- Windows, pages, dialogs, and reusable components.
- Navigation and role-specific interfaces.
- Forms, input controls, tables, and notifications.
- ViewModels, commands, and data binding.
- UI validation feedback and loading/error states.
- Integration with agreed backend service interfaces.
- UI unit tests.

Restrictions:
- Do not implement database queries or EF Core operations.
- Do not modify database schemas or migrations.
- Do not implement authentication or authorization logic.
- Do not implement business rules inside UI components or ViewModels.
- Do not duplicate backend validation.
- Do not modify backend service implementations unless explicitly coordinating an integration fix.

## BACKEND AGENT

**Primary responsibility:** Business logic, database, ORM, authentication, authorization, validation, exception handling, and services.

Allowed responsibilities:
- Entity models and database relationships.
- EF Core and LINQ queries.
- Database initialization and migrations.
- Repositories and service classes.
- Business rules and status management.
- Authentication and authorization.
- Guest-session identification.
- Centralized validation and exception handling.
- Transactions and audit logging.
- Backend unit and integration tests.
- Service interfaces and UI integration contracts.

Restrictions:
- Do not redesign or modify existing UI layouts.
- Do not change visual styling, navigation, or UI components.
- Do not duplicate ViewModel responsibilities.
- Do not put presentation logic inside backend services.
- Do not modify UI files unless explicitly coordinating an integration fix.

# 3. SHARED ARCHITECTURE AND INTEGRATION

Both agents MUST follow the same architectural boundaries.

### Responsibilities

| Component | Responsibility |
|---|---|
| UI | Presentation and user interaction |
| ViewModels | UI state, commands, and data binding |
| Services | Business logic and operations |
| Repositories | Data access and database queries |
| Entities | Database representation |
| DTOs | Data exchanged between layers |
| Validators | Input and business-data validation |
| Database | Persistent application data |

### Integration Rules

1. ViewModels communicate with backend services through agreed interfaces.
2. Backend services must not depend on UI components or ViewModels.
3. UI components must not directly access the database.
4. Use dependency injection where appropriate.
5. Agree on service method names, parameters, return types, and DTO structures.
6. Do not independently create competing implementations of shared interfaces.
7. Keep shared models and contracts consistent.
8. Handle asynchronous operations appropriately.
9. Backend errors must be returned in a format that the UI can display.
10. Changes to shared contracts must be communicated and documented before implementation.

### Shared Contract Requirements

Before integrating a feature, ensure both agents agree on:

- Entity and DTO property names.
- Data types and nullability.
- Service interfaces and method signatures.
- Expected return values.
- Validation error formats.
- Exception-handling behavior.
- Authentication and authorization requirements.
- Database relationships.

Never assume that similarly named classes or properties are automatically compatible.

# 4. DATABASE AND BUSINESS RULE CONSISTENCY

The Backend Agent owns database implementation. The UI Agent must follow the agreed data contracts.

Both agents must respect these business rules:

- Check-in → `Occupied`.
- Checkout → `Dirty` + automatically create housekeeping task.
- Cleaning started → `Cleaning`.
- Cleaning completed → `Available` only if all required checklist items are completed and no maintenance restriction exists.
- Approved maintenance → `OutOfService`.
- Restore availability only after maintenance resolution and approval.

Additional requirements:

- Prevent double bookings.
- Prevent invalid room and staff assignments.
- Prevent unauthorized status changes.
- Use transactions for important multi-record operations.
- Maintain audit logs for important operations.
- Use soft deletion/deactivation for operational records.
- Keep dashboard statistics and filtered records database-driven.
- Protect guest records through secure guest-session identification.

The UI may display available actions based on user roles, but backend authorization remains mandatory.

# 5. FEATURE AND DATA CONSISTENCY

Both agents must use consistent feature names and terminology.

### ADMIN
- Dashboard
- Rooms
- Guests
- Room Operations
- Staffs
- Maintenance

### RECEPTIONIST
- Dashboard
- Rooms
- Guests
- Housekeeping
- Account

### HOTEL KEEPER
- Dashboard
- Tasks
- Reports

### GUEST
- Dashboard
- Tasks
- Room Info
- Requests

Do not independently rename these modules, their data fields, or their operations without coordination.

# 6. VALIDATION AND ERROR-HANDLING CONSISTENCY

The Backend Agent owns actual validation and exception handling.

The UI Agent owns displaying validation feedback.

Requirements:

- Every user-input field must have appropriate validation.
- Use consistent validation rules across UI and backend.
- Display field-specific errors.
- Handle duplicate records, invalid formats, database failures, and concurrency conflicts.
- Never expose passwords, credentials, or technical stack traces.
- Do not treat UI validation as a replacement for backend validation.
- Ensure backend validation errors map correctly to their corresponding UI fields.

# 7. SHARED FILE PROTECTION

Some files may be required by both agents.

Before modifying a shared file:

1. Inspect its existing implementation.
2. Determine which agent owns its responsibilities.
3. Check whether the other agent depends on it.
4. Coordinate changes to avoid overwriting existing work.
5. Preserve existing functionality.
6. Document any shared-file modifications in `UPDATES.md`.

Do not create duplicate:
- Entities.
- DTOs.
- Service interfaces.
- ViewModels.
- Database contexts.
- Validation helpers.
- Authentication handlers.
- Shared utilities.

If an existing implementation already provides the required functionality, extend it instead of creating another version.

# 8. MERGE PREPARATION

Both agents MUST prepare their implementations for integration.

### UI Agent Requirements

- Keep UI files separate from backend implementation.
- Use agreed service interfaces and DTOs.
- Identify any temporary mock or sample data.
- Document required backend methods.
- Identify missing backend dependencies.
- Ensure ViewModels are ready for actual backend integration.
- Do not leave unfinished functionality disguised as working features.

### Backend Agent Requirements

- Keep backend services independent of UI implementation.
- Provide complete service interfaces and DTOs.
- Document database entities and relationships.
- Identify required UI integration methods.
- Ensure database operations and business rules are implemented.
- Identify any missing UI dependencies.
- Do not report unimplemented methods as completed.

### Merge Checklist

Before considering the project merge-ready, verify:

- [ ] No conflicting class or namespace definitions.
- [ ] No duplicate entities, DTOs, or service interfaces.
- [ ] Consistent property names and data types.
- [ ] Consistent service method signatures.
- [ ] Correct dependency injection registrations.
- [ ] Correct database relationships and configurations.
- [ ] ViewModels connect to the correct backend services.
- [ ] Authentication and authorization work correctly.
- [ ] Validation errors display correctly.
- [ ] All role-specific features use the correct data.
- [ ] No accidental removal of existing functionality.
- [ ] No unresolved references or missing dependencies.
- [ ] Application builds successfully.
- [ ] Application runs successfully.
- [ ] Important integration tests pass.

# 9. UPDATES.md — DEVELOPMENT TRACKING

Both agents MUST maintain `UPDATES.md`.

Use separate sections to prevent overwriting each other's progress.

Required format:

## UI Agent
- Completed:
- In Progress:
- Unfinished:
- Files Modified:
- Backend Dependencies:
- Issues:

## Backend Agent
- Completed:
- In Progress:
- Unfinished:
- Files Modified:
- UI Dependencies:
- Database Changes:
- Issues:

## Integration and Merge
- Compatible Components:
- Pending Integration:
- Conflicts:
- Build Status:
- Test Results:
- Remaining Tasks:

Rules:
1. Update progress after meaningful implementation milestones.
2. Never overwrite or remove the other agent's updates.
3. Record actual changes, not planned or assumed changes.
4. Clearly identify unfinished functionality.
5. Record important file modifications.
6. Document breaking changes to shared contracts.
7. Report tests only when actually executed.

# 10. BUILD, TESTING, AND COMPLETION

Both agents must verify their own implementation independently.

UI Agent:
- Verify UI builds.
- Verify navigation and role-specific screens.
- Verify ViewModels and data binding.
- Verify forms, commands, dialogs, and UI feedback.
- Identify temporary mock data.

Backend Agent:
- Verify backend builds.
- Verify EF Core configurations and ORM queries.
- Verify database operations.
- Verify business rules and status transitions.
- Verify authentication and authorization.
- Verify validation and exception handling.
- Run available unit and integration tests.

Final integration:
- Build the combined project.
- Resolve integration errors.
- Verify database connectivity.
- Test important end-to-end workflows.
- Verify that UI and backend communicate correctly.
- Report actual build and test results.

# 11. FINAL INSTRUCTION

The UI Agent and Backend Agent are separate contributors to ONE application, not two independent applications.

Their implementations must remain compatible.

**DO NOT:**
- Rebuild the entire project.
- Overwrite another agent's implementation.
- Duplicate shared functionality.
- Change agreed contracts without coordination.
- Introduce incompatible data structures.
- Claim unfinished work is complete.
- Ignore existing project requirements.

**PRIORITY:**

1. Preserve existing functionality.
2. Maintain separation of responsibilities.
3. Keep shared contracts consistent.
4. Prevent conflicting modifications.
5. Prepare both implementations for integration.
6. Ensure the final merged application is buildable, runnable, and functional.

Always prioritize a clean, compatible merge over unnecessary restructuring or rewriting.