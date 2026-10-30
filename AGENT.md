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


# 3. FEATURE AND DATA CONSISTENCY

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

# 4. VALIDATION AND ERROR-HANDLING CONSISTENCY

Requirements:

- Every user-input field must have appropriate validation.
- Use consistent validation rules across UI and backend.
- Display field-specific errors.
- Handle duplicate records, invalid formats, database failures, and concurrency conflicts.
- Never expose passwords, credentials, or technical stack traces.
- Do not treat UI validation as a replacement for backend validation.
- Ensure backend validation errors map correctly to their corresponding UI fields.



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
