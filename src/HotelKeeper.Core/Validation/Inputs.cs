using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using HotelKeeper.Data;

namespace HotelKeeper.Validation;

/// <summary>
/// Length limits that mirror the database schema. Keeping them in one place means the UI, services and
/// database agree about what is valid, and a user never sees a truncation/constraint error.
/// </summary>
public static class InputLimits
{
    public const int Username = 50;
    public const int FullName = 100;
    public const int Email = 120;
    public const int Phone = 40;
    public const int Address = 255;
    public const int Nationality = 60;
    public const int IdDocument = 80;
    public const int Notes = 255;
    public const int RoomNumber = 10;
    public const int Building = 40;
    public const int RoomTypeName = 40;
    public const int Description = 255;
    public const int Amenities = 255;
    public const int ItemName = 60;
    public const int ItemCategory = 40;
    public const int PaymentReference = 80;
    public const int RequestDescription = 500;
    public const int Resolution = 500;
    public const int ChecklistLabel = 100;
    public const int SettingValue = 255;
    public const int CsvList = 500;
    public const int Search = 100;
    public const int Password = 128;
}

/// <summary>
/// Reusable, field-aware validation used by every service. Rules throw <see cref="DomainException"/>
/// with a message that is safe to display to the user; UI controls can additionally validate before submit.
/// </summary>
public static class Input
{
    static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex UsernamePattern = new(@"^[A-Za-z0-9._-]{3,50}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex PhonePattern = new(@"^[0-9+()\-.\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex RoomNumberPattern = new(@"^[A-Za-z0-9][A-Za-z0-9\-/]{0,9}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string Required(string? value, string field, int maxLength)
        => Required(value, field, 1, maxLength);

    public static string Required(string? value, string field, int minLength, int maxLength)
    {
        var text = (value ?? "").Trim();
        if (text.Length == 0) throw new DomainException($"{field} is required.");
        if (text.Length < minLength) throw new DomainException($"{field} must be at least {minLength} characters.");
        if (text.Length > maxLength) throw new DomainException($"{field} must be at most {maxLength} characters.");
        return text;
    }

    public static string Optional(string? value, string field, int maxLength)
    {
        var text = (value ?? "").Trim();
        if (text.Length > maxLength) throw new DomainException($"{field} must be at most {maxLength} characters.");
        return text;
    }

    public static string? OptionalOrNull(string? value, string field, int maxLength)
    {
        var text = Optional(value, field, maxLength);
        return text.Length == 0 ? null : text;
    }

    /// <summary>Normalizes optional search text and enforces the same length limit across service queries.</summary>
    public static string? Search(string? value, int maxLength = InputLimits.Search)
    {
        var text = Optional(value, "Search", maxLength);
        return text.Length == 0 ? null : text;
    }

    public static string Email(string? value, bool required, string field = "Email")
    {
        var text = required ? Required(value, field, InputLimits.Email) : Optional(value, field, InputLimits.Email);
        if (text.Length > 0 && !EmailPattern.IsMatch(text))
            throw new DomainException($"Enter a valid {field.ToLowerInvariant()} address (for example name@example.com).");
        return text;
    }

    public static string Phone(string? value, bool required, string field = "Phone")
    {
        var text = required ? Required(value, field, InputLimits.Phone) : Optional(value, field, InputLimits.Phone);
        if (text.Length > 0)
        {
            if (!PhonePattern.IsMatch(text) || text.Count(char.IsDigit) < 3)
                throw new DomainException($"Enter a valid {field.ToLowerInvariant()} number.");
        }
        return text;
    }

    public static string Username(string? value)
    {
        var text = Required(value, "Username", 3, InputLimits.Username);
        if (!UsernamePattern.IsMatch(text))
            throw new DomainException("Username must be 3–50 characters using only letters, digits, dots, underscores or hyphens.");
        return text;
    }

    /// <summary>Builds a readable unique username when the caller leaves it blank.</summary>
    public static string UsernameFromName(string fullName)
    {
        var prefix = new string(fullName.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (prefix.Length > 24) prefix = prefix[..24];
        if (prefix.Length < 3) prefix = "GUEST";
        return prefix + RandomNumberGenerator.GetInt32(1000, 10000).ToString(CultureInfo.InvariantCulture);
    }

    public static string RoomNumber(string? value)
    {
        var text = Required(value, "Room number", InputLimits.RoomNumber);
        if (!RoomNumberPattern.IsMatch(text))
            throw new DomainException("Room number must start with a letter or digit and use only letters, digits, hyphens or slashes.");
        return text;
    }

    /// <summary>ID document types with the format their numbers must follow.</summary>
    public enum IdDocumentType { Passport, NationalId, DriverLicense, IdCard, ResidentPermit, Other }

    public static readonly string[] IdDocumentTypes = {"Passport","National ID","Driver license","ID card","Resident permit","Other"};

    public static string IdDocumentNumber(string? value, IdDocumentType type)
    {
        var text = Required(value, "ID number", InputLimits.IdDocument);
        text = text.Trim();
        return type switch
        {
            IdDocumentType.Passport => text.Length >= 6 ? text.ToUpperInvariant() : "Passport numbers are at least 6 characters.",
            IdDocumentType.NationalId => text.Length >= 7 ? text.ToUpperInvariant() : "National ID numbers are at least 7 characters.",
            IdDocumentType.DriverLicense => ValidateLetterDigitDash(text, 12, 16, "Driver license"),
            IdDocumentType.IdCard => ValidateLetterDigitDash(text, 8, 12, "ID card"),
            IdDocumentType.ResidentPermit => ValidateLetterDigitDash(text, 9, 13, "Resident permit"),
            IdDocumentType.Other => text,
            _ => text
        };
    }

    static string ValidateLetterDigitDash(string text, int min, int max, string field)
    {
        if (text.Length < min || text.Length > max)
            throw new DomainException($"{field} must be {min}-{max} characters using letters, digits and single dashes.");
        var segments = text.Split('-');
        if (segments.Length > 3) throw new DomainException($"{field} may use at most two single dashes.");
        foreach (var segment in segments)
            if (segment.Length == 0) throw new DomainException($"{field} may not start, end or have two dashes next to each other.");
        return text;
    }

    public static string OneOf(string? value, IReadOnlyCollection<string> allowed, string field)
    {
        var text = Required(value, field, 1, 80);
        if (!allowed.Contains(text, StringComparer.Ordinal))
            throw new DomainException($"Choose a valid {field.ToLowerInvariant()}.");
        return text;
    }

    public static string? OptionalOneOf(string? value, IReadOnlyCollection<string> allowed, string field)
    {
        var text = Optional(value, field, 80);
        if (text.Length == 0) return null;
        if (!allowed.Contains(text, StringComparer.Ordinal))
            throw new DomainException($"Choose a valid {field.ToLowerInvariant()}.");
        return text;
    }

    public static int InRange(int value, int min, int max, string field)
    {
        if (value < min || value > max) throw new DomainException($"{field} must be between {min} and {max}.");
        return value;
    }

    public static int PositiveId(int value, string field)
    {
        if (value <= 0) throw new DomainException($"Choose a valid {field.ToLowerInvariant()}.");
        return value;
    }

    public static decimal Money(decimal value, decimal min, decimal max, string field)
    {
        if (decimal.Round(value, 2) != value)
            throw new DomainException($"{field} can have at most two decimal places.");
        if (value < min || value > max) throw new DomainException($"{field} must be between {min:0.00} and {max:0.00}.");
        return value;
    }

    public static DateTime Date(DateTime value, string field) => value.Date;
}
