# Guest Registration — No Password, Stay Duration, and ID Validation

Inspect the existing project, database, `AGENTS.md`, and `UPDATES.md` before making changes.

Implement and correct the Guest Registration feature according to the following requirements.

## 1. Guest Registration

Guests must NOT have passwords or independently created login accounts.

The registration form should collect:

- Full Name
- Contact Number
- Email Address (optional)
- Address (if currently supported)
- ID Document Type
- ID Document Number
- Number of Days to Stay

Requirements:
- Allow staff to register guests without creating a password.
- The stay duration must be entered as a number of days.
- Validate that the number of days is a positive whole number.
- Store guest information as a guest record.
- Do not require room assignment during registration.
- Guests will be assigned to rooms separately through the **Rooms** menu.
- Prevent duplicate or invalid guest records according to the existing database rules.

## 2. ID Document Dropdown and Dynamic Validation

Replace the plain ID document text field with a dropdown selection.

Include appropriate Philippine ID document types, such as:

- Philippine National ID
- Passport
- Driver's License
- UMID
- PhilHealth ID
- Postal ID
- Voter's ID
- Other

When a document type is selected, the ID Number input must adapt to that document's expected format.

Examples:
- Numeric-only formats where applicable.
- Alphanumeric formats where applicable.
- Hyphens or dashes where applicable.
- Appropriate length limits and validation for each document type.

Requirements:
- Apply the correct input rules based on the selected document type.
- Display a helpful example or placeholder for the selected type.
- Reject invalid characters and incorrect formats.
- Do not apply one universal ID format to every document.
- Allow legitimate document variations where applicable.
- For Other, use flexible validation without an unnecessarily restrictive mask.

## 3. Guest List and Descriptions

The Guests page must display all registered guest records.

Include useful guest information, such as:
- Guest Name
- Contact Number
- Email
- ID Document Type
- ID Document Number (mask sensitive digits in list views where appropriate)
- Stay Duration
- Assigned Room, if any
- Current Guest/Stay Status, if applicable

Requirements:
- Allow staff to view the complete guest record through a details view.
- Support searching and filtering guest records.
- Display unassigned guests clearly.
- Keep guest registration and room assignment as separate operations.

## 4. Room Assignment Integration

Room assignment must happen through the Rooms menu.

- Display registered, unassigned guests when assigning a guest to a room.
- Allow staff to select an existing guest.
- Save the guest-to-room assignment.
- Use the entered stay duration to calculate the expected checkout date based on the actual check-in date.
- Prevent assigning the same guest to multiple active room stays.
- Prevent assigning multiple guests beyond the room's allowed capacity.
- Ensure room status and guest assignment remain synchronized.

## 5. Backend and Database

- Update entities, DTOs, validation, services, repositories, and database relationships as necessary.
- Guest records must not contain password fields or require password hashing.
- Do not create guest authentication accounts.
- Store the selected ID document type and its corresponding number.
- Store stay duration and relevant check-in/expected checkout dates.
- Use transactions for guest assignment and room status changes.
- Preserve existing staff authentication and authorization.

## 6. Restrictions

- Do not add guest passwords or guest self-registration.
- Do not add a separate booking or reservation workflow.
- Do not require room assignment while registering a guest.
- Do not break existing Rooms, Guests, Check-in, or Checkout functionality.
- Modify the existing implementation instead of rebuilding unrelated features.
- Keep UI and backend contracts consistent.

## 7. Verification

Test the following:
- Register a guest without a password.
- Register guests with different ID document types.
- Validate correct and incorrect ID number formats.
- Reject invalid stay durations.
- Display all registered guest records.
- Search and view guest details.
- Assign an existing guest through the Rooms menu.
- Calculate the expected checkout date correctly.
- Prevent duplicate active assignments and room-capacity violations.

Update `UPDATES.md` with completed work, unfinished work, modified files, database changes, and actual test results.

**Final requirement:** Guests are registered as records, not user accounts. Staff register guests first, then assign them to rooms through the Rooms menu.