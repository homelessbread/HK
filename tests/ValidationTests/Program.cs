using System.Data.Common;
using HotelKeeper.Data;
using HotelKeeper.Models;
using HotelKeeper.Services;
using HotelKeeper.Validation;
using HotelKeeper.ViewModels;

// Dependency-free unit checks for validation, password handling and user-facing error text.
int fails = 0;
void Ok(bool condition, string what) { Console.WriteLine((condition ? "PASS  " : "FAIL  ") + what); if (!condition) fails++; }
void Throws(Action action, string what)
{
    try { action(); Ok(false, what + " (no error)"); }
    catch (DomainException ex) { Ok(true, what + " -> " + ex.Message); }
}

// ---- Input: required/optional/length ---------------------------------------------------------------
Throws(() => Input.Required(null, "Full name", 100), "null required text rejected");
Throws(() => Input.Required("   ", "Full name", 100), "blank required text rejected");
Throws(() => Input.Required("A", "Full name", 2, 100), "too-short required text rejected");
Throws(() => Input.Required(new string('x', 101), "Full name", 100), "too-long required text rejected");
Ok(Input.Required("  Ana Cruz  ", "Full name", 2, 100) == "Ana Cruz", "required text is trimmed");
Ok(Input.Optional(null, "Address", 255) == "", "null optional text becomes empty");
Ok(Input.OptionalOrNull("   ", "Notes", 255) is null, "blank optional text becomes null");
Throws(() => Input.Optional(new string('x', 256), "Address", 255), "too-long optional text rejected");
Ok(Input.Search("  room 12  ") == "room 12", "search text is trimmed");
Ok(Input.Search("   ") is null, "blank search text becomes no filter");
Throws(() => Input.Search(new string('x', InputLimits.Search + 1)), "overlong search text rejected");

// ---- Input: formats --------------------------------------------------------------------------------
Ok(Input.Email("name@example.com", required: false) == "name@example.com", "valid email accepted");
Throws(() => Input.Email("not-an-email", required: false), "malformed email rejected");
Throws(() => Input.Email("", required: true), "missing required email rejected");
Ok(Input.Phone("+63 (917) 555-1234", required: false).Length > 0, "valid phone accepted");
Throws(() => Input.Phone("call me", required: false), "malformed phone rejected");
Throws(() => Input.Phone("12", required: false), "phone with too few digits rejected");
Ok(Input.Username("ana.cruz_1") == "ana.cruz_1", "valid username accepted");
Throws(() => Input.Username("ab"), "too-short username rejected");
Throws(() => Input.Username("has space"), "username with a space rejected");
Throws(() => Input.Username(new string('a', 51)), "too-long username rejected");
Ok(Input.Username(Input.UsernameFromName("Ana Cruz")).Length > 3, "generated username is valid");
Ok(Input.RoomNumber("12-A/2") == "12-A/2", "valid room number accepted");
Throws(() => Input.RoomNumber("bad room!"), "malformed room number rejected");

// ---- Input: ranges and money -----------------------------------------------------------------------
Ok(Input.InRange(5, 1, 10, "Guests") == 5, "in-range integer accepted");
Throws(() => Input.InRange(0, 1, 10, "Guests"), "below-minimum integer rejected");
Throws(() => Input.InRange(11, 1, 10, "Guests"), "above-maximum integer rejected");
Throws(() => Input.PositiveId(0, "guest"), "zero id rejected");
Throws(() => Input.PositiveId(-1, "guest"), "negative id rejected");
Ok(Input.Money(10.50m, 0.01m, 100m, "Amount") == 10.50m, "two-decimal money accepted");
Throws(() => Input.Money(10.555m, 0.01m, 100m, "Amount"), "money with three decimals rejected");
Throws(() => Input.Money(0m, 0.01m, 100m, "Amount"), "money below minimum rejected");
Throws(() => Input.Money(101m, 0.01m, 100m, "Amount"), "money above maximum rejected");
Ok(Input.OneOf("High", Priorities.All, "Priority") == "High", "allowed choice accepted");
Throws(() => Input.OneOf("Extreme", Priorities.All, "Priority"), "unknown choice rejected");

// ---- Passwords -------------------------------------------------------------------------------------
var hash = Passwords.Hash("StrongPass1");
Ok(Passwords.Verify("StrongPass1", hash), "correct password verifies");
Ok(!Passwords.Verify("WrongPass1", hash), "wrong password fails");
Ok(!Passwords.Verify(null, hash), "null password fails safely");
Ok(!Passwords.Verify("StrongPass1", null), "null hash fails safely");
Ok(!Passwords.Verify("StrongPass1", "not-a-hash"), "malformed hash fails safely");
Ok(!Passwords.Verify("StrongPass1", "1.bad.hash"), "invalid base64 hash fails safely");
Ok(!Passwords.Verify("StrongPass1", "99999999.AAAA.AAAA"), "absurd iteration count fails safely");
Ok(Passwords.Validate(null) != null, "null password rejected");
Ok(Passwords.Validate("short") != null, "weak password rejected");
Ok(Passwords.Validate("alllowercase1") != null, "password without upper case rejected");
Ok(Passwords.Validate("ALLUPPERCASE1") != null, "password without lower case rejected");
Ok(Passwords.Validate("NoDigitsHere") != null, "password without a digit rejected");
Ok(Passwords.Validate(new string('A', 129) + "a1") != null, "over-long password rejected");
Ok(Passwords.Validate("StrongPass1") is null, "strong password accepted");
Ok(Passwords.Validate(Passwords.Generate()) is null, "generated temporary password is strong");

// ---- Business rules --------------------------------------------------------------------------------
ReservationRules.ValidateDates(DateTime.Today, DateTime.Today.AddDays(2), 2);
Ok(true, "valid reservation dates accepted");
Throws(() => ReservationRules.ValidateDates(DateTime.Today.AddDays(-1), DateTime.Today, 1), "past check-in rejected");
Throws(() => ReservationRules.ValidateDates(DateTime.Today, DateTime.Today, 1), "same-day check-out rejected");
Throws(() => ReservationRules.ValidateDates(DateTime.Today, DateTime.Today.AddDays(ReservationRules.MaxNights + 1), 1), "stay over the night limit rejected");
Throws(() => ReservationRules.ValidateDates(DateTime.Today, DateTime.Today.AddDays(1), 0), "zero guests rejected");
Throws(() => ReservationRules.ValidateDates(DateTime.Today, DateTime.Today.AddDays(1), ReservationRules.MaxGuests + 1), "too many guests rejected");
Throws(() => ReservationRules.ValidateDates(DateTime.Today.AddYears(3), DateTime.Today.AddYears(3).AddDays(1), 1), "check-in too far ahead rejected");
Ok(ReservationRules.ValidateNotes("  late arrival  ") == "late arrival", "reservation notes are trimmed");
Throws(() => ReservationRules.ValidateNotes(new string('x', InputLimits.Notes + 1)), "too-long reservation notes rejected");

Ok(RoomRules.CheckManual(Roles.Receptionist, RoomStatus.Available, RoomStatus.Dirty) is null, "receptionist may flag an available room dirty");
Ok(RoomRules.CheckManual(Roles.Admin, RoomStatus.OutOfService, RoomStatus.Dirty) is null, "admin may clear an out-of-service room to dirty");
Ok(RoomRules.CheckManual(Roles.Receptionist, RoomStatus.Occupied, RoomStatus.Available) != null, "occupied room cannot be changed manually");
Ok(RoomRules.CheckManual(Roles.Receptionist, RoomStatus.OutOfService, RoomStatus.Dirty) != null, "receptionist cannot clear an out-of-service room");
Ok(RoomRules.CheckManual(Roles.Receptionist, RoomStatus.Dirty, RoomStatus.Occupied) != null, "manual move to occupied is refused");
Ok(PaymentRules.Methods.Contains("Cash") && PaymentRules.Statuses.Contains("Paid"), "payment constants are usable by the UI");

// ---- Form and filter input -------------------------------------------------------------------------
var requiredField = FormField.Txt("name", "Full name", required: true);
Ok(!requiredField.TryGet(out _) && requiredField.HasError, "empty required form field rejected");
requiredField.Text = "Ana";
Ok(requiredField.TryGet(out var nameValue) && (string?)nameValue == "Ana", "valid form field accepted");
var limitedField = FormField.Txt("name", "Name", maxLength: 5);
limitedField.Text = "123456";
Ok(!limitedField.TryGet(out _) && limitedField.Error.Contains("5"), "form max length enforced");
var intField = FormField.Int("n", "Guests", 1, min: 1, max: 20);
intField.Text = "21";
Ok(!intField.TryGet(out _) && intField.Error.Contains("20"), "form integer maximum enforced");
intField.Text = "abc";
Ok(!intField.TryGet(out _) && intField.Error.Contains("whole"), "form integer format enforced");
var moneyField = FormField.Dec("rate", "Rate", 10m, min: 1m, max: 100m);
Ok(moneyField.TryGet(out var rateValue) && (decimal?)rateValue == 10m, "form decimal accepted");

var numericFilter = new FilterVm { Key = "n", Label = "Guests", Kind = FilterKind.Integer, Min = 1, Max = 20, Text = "abc" };
Ok(!numericFilter.TryGetValue(out _) && numericFilter.HasError, "non-numeric filter rejected");
Ok(numericFilter.Error.Contains("whole"), "non-numeric filter shows a field-specific message");
numericFilter.Text = "0";
Ok(!numericFilter.TryGetValue(out _) && numericFilter.Error.Contains("1"), "below-minimum filter rejected");
numericFilter.Text = "21";
Ok(!numericFilter.TryGetValue(out _) && numericFilter.Error.Contains("20"), "above-maximum filter rejected");
numericFilter.Text = "5";
Ok(numericFilter.TryGetValue(out var filterValue) && (int?)filterValue == 5, "valid filter accepted");
numericFilter.Text = "";
Ok(numericFilter.TryGetValue(out var emptyValue) && emptyValue is null, "empty numeric filter means no filter");

// ---- User-facing errors never leak implementation details ------------------------------------------
Ok(ErrorText.For(new DomainException("Enter a valid email address.")) == "Enter a valid email address.", "business message shown unchanged");
Ok(ErrorText.For(new AuthorizationException("You do not have permission to do this.")) == "You do not have permission to do this.", "authorization message shown");
var dbText = ErrorText.For(new FakeDbException("Table 'hotelkeeper.users' contains a duplicate key"));
Ok(!dbText.Contains("hotelkeeper", StringComparison.OrdinalIgnoreCase) && !dbText.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
   && dbText.Contains("database", StringComparison.OrdinalIgnoreCase), "database exception details sanitized");
var unexpectedText = ErrorText.For(new Exception("C:\\Users\\secret\\stack trace"));
Ok(!unexpectedText.Contains("secret", StringComparison.OrdinalIgnoreCase) && unexpectedText.Contains("went wrong"), "unexpected exception details sanitized");
Ok(ErrorText.For(new OperationCanceledException()).Contains("cancelled"), "cancellation reported clearly");
Ok(ErrorText.For(new TimeoutException()).Contains("too long"), "timeout reported clearly");

var marker = "SECRET-" + Guid.NewGuid().ToString("N");
AppLog.Error("unit-test", new Exception($"Server=localhost;User=root;Password={marker};"));
var logText = File.Exists(AppLog.LogFile) ? File.ReadAllText(AppLog.LogFile) : "";
Ok(logText.Contains("Password=***") && !logText.Contains(marker, StringComparison.Ordinal), "log file redacts connection-string passwords");

// ---- Room states: the state classes must behave exactly like the original switch they replaced ------
static string? LegacyCheck(string role, string from, string to)
{
    if (from == to) return $"Room is already {Labels.Of(to)}.";
    if (from == RoomStatus.Occupied) return "An occupied room changes status only through check-out.";

    return (from, to) switch
    {
        (RoomStatus.Available, RoomStatus.Dirty) when role is Roles.Receptionist or Roles.Admin => null,
        (RoomStatus.Available or RoomStatus.Dirty or RoomStatus.Cleaning, RoomStatus.OutOfService) when role is Roles.Receptionist or Roles.Admin => null,
        (RoomStatus.OutOfService, RoomStatus.Dirty) when role == Roles.Admin => null,
        (RoomStatus.OutOfService, _) => "Only an admin can clear a room that is Out of Service, and it returns as Dirty so it is cleaned first.",
        (_, RoomStatus.Occupied) => "A room becomes Occupied only through check-in.",
        (_, RoomStatus.Cleaning) => "A room moves to Cleaning when a housekeeper starts its cleaning task.",
        (_, RoomStatus.Available) => "A room becomes Available only when housekeeping completes its cleaning task.",
        _ => "That status change is not allowed."
    };
}

var stateRoles = new[] { Roles.Admin, Roles.Receptionist, Roles.Housekeeper, Roles.Guest, "Nobody" };
var stateNames = RoomStatus.All.Append("Mystery").ToArray();
var stateMismatches = 0;
foreach (var stateRole in stateRoles)
    foreach (var fromStatus in stateNames)
        foreach (var toStatus in stateNames)
            if (LegacyCheck(stateRole, fromStatus, toStatus) != RoomRules.CheckManual(stateRole, fromStatus, toStatus)) stateMismatches++;
Ok(stateMismatches == 0, $"room state classes match the original rules for all {stateRoles.Length * stateNames.Length * stateNames.Length} role/status combinations");

// ---- Entities: encapsulation, inheritance, polymorphism and field-level errors ----------------------
var newGuest = Guest.Register("Ana Cruz", "ana@example.com", "", "Quezon City", "Filipino", "Passport", "p1234567", 2);
Ok(newGuest.IsNew && newGuest.FullName == "Ana Cruz" && newGuest.IdDocument == "P1234567", "Guest.Register builds a validated, normalised guest");

Person asPerson = newGuest;
var receptionist = new StaffMember(7, "RECEP1", "Rita Santos", "rita@example.com", "", Roles.Receptionist, true);
Ok(asPerson.Kind == "Guest" && receptionist.Kind == Roles.Receptionist && asPerson.DisplayName != receptionist.DisplayName,
   "Person subclasses answer Kind and DisplayName for themselves");
Ok(receptionist.CanManageRooms && !new StaffMember(8, "HK1", "Hana Lim", "", "12345", Roles.Housekeeper, true).CanManageRooms, "staff role decides room permissions");

try { Guest.Register("A", "", "12345", "", "", "", "", 1); Ok(false, "short name rejected (no error)"); }
catch (FieldValidationException ex) { Ok(ex.Field == "Full name", "invalid name reports its own field -> " + ex.Message); }
try { Guest.Register("Ana Cruz", "", "", "", "", "", "", 1); Ok(false, "missing contact rejected (no error)"); }
catch (FieldValidationException ex) { Ok(ex.Field == "Phone", "missing phone/email reports the phone field -> " + ex.Message); }
try { Guest.Register("Ana Cruz", "ana@example.com", "", "", "", "", "", 0); Ok(false, "zero days rejected (no error)"); }
catch (FieldValidationException ex) { Ok(ex.Field == "Number of days to stay", "invalid stay length reports its own field -> " + ex.Message); }
Throws(() => Guest.Register("Ana Cruz", "not-an-email", "", "", "", "", "", 1), "field errors are still DomainExceptions, so existing handling keeps working");

var g5a = new Guest(5, null, "Ana Cruz", "", "12345", "", "", "", "", 1, false);
var g5b = new Guest(5, null, "Ana C.", "", "12345", "", "", "", "", 1, false);
var s5 = new StaffMember(5, "S5", "Sam Reyes", "", "12345", Roles.Admin, true);
Ok(g5a.Equals(g5b) && !g5a.Equals(s5) && !newGuest.Equals(Guest.Register("Ana Cruz", "ana@example.com", "", "", "", "", "", 2)),
   "entities are equal by kind and id; unsaved ones only equal themselves");

var resv = Reservation.Create(1, 2, DateTime.Today.AddDays(1), DateTime.Today.AddDays(4), 2, 100m, "  late arrival ", confirmed: false);
Ok(resv.Nights == 3 && resv.Total == 300m && resv.Status == ResStatus.Pending && resv.Notes == "late arrival", "Reservation.Create computes nights, total and status");
Ok(resv.Overlaps(DateTime.Today.AddDays(3), DateTime.Today.AddDays(6)) && !resv.Overlaps(DateTime.Today.AddDays(4), DateTime.Today.AddDays(6)), "reservation overlap follows the availability rule");
resv.Confirm();
Ok(resv.Status == ResStatus.Confirmed, "a pending reservation can be confirmed");
Throws(() => resv.Confirm(), "confirming twice is rejected");
resv.Cancel();
Throws(() => resv.Cancel(), "cancelling twice is rejected");
Throws(() => Reservation.Create(1, 2, DateTime.Today.AddDays(-1), DateTime.Today, 1, 100m, null, false), "Reservation.Create reuses the reservation date rules");

var occupiedRoom = new Room(3, "101", "Main", 1, 1, "Standard", 2, 1500m, RoomStatus.Occupied, false);
Ok(occupiedRoom.State.Name == RoomStatus.Occupied && occupiedRoom.CheckManualChange(Roles.Admin, RoomStatus.Available) != null, "a room asks its state whether a manual change is allowed");
Ok(occupiedRoom.WhyNotReservable(DateTime.Today.AddDays(2), 3, DateTime.Today) != null && occupiedRoom.WhyNotReservable(DateTime.Today.AddDays(2), 2, DateTime.Today) is null,
   "a room reports why it cannot be reserved");

Console.WriteLine(fails == 0 ? "ALL PASSED" : $"{fails} FAILED");
return fails;

sealed class FakeDbException(string message) : DbException(message);
