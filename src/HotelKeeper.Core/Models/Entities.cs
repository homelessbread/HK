using HotelKeeper.Data;
using HotelKeeper.Validation;

namespace HotelKeeper.Models;

/// <summary>Something that is archived instead of deleted, so its history stays on record.</summary>
public interface IArchivable
{
    bool IsArchived { get; }
}

/// <summary>
/// Base class of every domain object. It has an identity: two objects are the same when they are the
/// same kind of object with the same id.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    protected Entity(int id) => Id = id;

    /// <summary>Database id; 0 for an object that has not been saved yet.</summary>
    public int Id { get; }

    public bool IsNew => Id == 0;

    public bool Equals(Entity? other) =>
        other is not null && (ReferenceEquals(this, other) || (Id != 0 && Id == other.Id && GetType() == other.GetType()));

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => Id == 0 ? base.GetHashCode() : HashCode.Combine(GetType(), Id);

    /// <summary>Runs a validation rule and, if it fails, reports which field the problem belongs to.</summary>
    protected static T Validated<T>(string field, Func<T> rule)
    {
        try { return rule(); }
        catch (FieldValidationException) { throw; }
        catch (DomainException ex) { throw new FieldValidationException(field, ex.Message); }
    }
}

/// <summary>
/// A human being the hotel keeps a record of. Guests and staff share the name and contact rules here and
/// differ in <see cref="Kind"/>, which every subclass must answer for itself.
/// </summary>
public abstract class Person : Entity
{
    /// <summary>Rebuilds a person from stored data. Stored values are trusted; new values go through the validating methods.</summary>
    protected Person(int id, string fullName, string email, string phone) : base(id)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
    }

    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }

    /// <summary>What sort of person this is, e.g. "Guest" or "Receptionist".</summary>
    public abstract string Kind { get; }

    public virtual string DisplayName => $"{FullName} ({Kind})";

    public void Rename(string fullName) =>
        FullName = Validated("Full name", () => Input.Required(fullName, "Full name", 2, InputLimits.FullName));

    /// <summary>Changes the contact details. At least one of phone or email must remain.</summary>
    public void ChangeContact(string? email, string? phone)
    {
        var newEmail = Validated("Email", () => Input.Email(email, required: false));
        var newPhone = Validated("Phone", () => Input.Phone(phone, required: false));
        if (newEmail.Length == 0 && newPhone.Length == 0)
            throw new FieldValidationException("Phone", "Enter a phone number or an email address.");
        Email = newEmail;
        Phone = newPhone;
    }
}

/// <summary>A guest record. A guest may or may not have a sign-in account.</summary>
public sealed class Guest : Person, IArchivable
{
    public Guest(int id, int? userId, string fullName, string email, string phone, string address, string nationality,
        string idDocumentType, string idDocument, int stayDuration, bool isArchived) : base(id, fullName, email, phone)
    {
        UserId = userId;
        Address = address;
        Nationality = nationality;
        IdDocumentType = idDocumentType;
        IdDocument = idDocument;
        StayDuration = stayDuration;
        IsArchived = isArchived;
    }

    public int? UserId { get; }
    public string Address { get; }
    public string Nationality { get; }
    public string IdDocumentType { get; }
    public string IdDocument { get; }
    public int StayDuration { get; }
    public bool IsArchived { get; private set; }

    public override string Kind => "Guest";

    public bool HasAccount => UserId.HasValue;

    /// <summary>
    /// Creates a new, validated guest. Each failure names the field it belongs to
    /// (<see cref="FieldValidationException.Field"/>), in the same order the service has always checked them.
    /// </summary>
    public static Guest Register(string? fullName, string? email, string? phone, string? address, string? nationality,
        string? idDocumentType, string? idDocument, int stayDuration)
    {
        var name = Validated("Full name", () => Input.Required(fullName, "Full name", 2, InputLimits.FullName));
        var mail = Validated("Email", () => Input.Email(email, required: false));
        var tel = Validated("Phone", () => Input.Phone(phone, required: false));
        var addr = Validated("Address", () => Input.Optional(address, "Address", InputLimits.Address));
        var nat = Validated("Nationality", () => Input.Optional(nationality, "Nationality", InputLimits.Nationality));
        var idType = Validated("ID document type", () => Input.Optional(idDocumentType, "ID document type", 40));
        var rawNumber = Validated("ID document number", () => Input.Optional(idDocument, "ID document", InputLimits.IdDocument));
        if (stayDuration < 1)
            throw new FieldValidationException("Number of days to stay", "Stay duration must be a positive whole number of days.");

        var number = rawNumber;
        if (number.Length > 0)
            number = Validated("ID document number", () => Input.IdDocumentNumber(rawNumber, IdTypeFrom(idType)));

        if (tel.Length == 0 && mail.Length == 0)
            throw new FieldValidationException("Phone", "Enter a phone number or an email address.");

        return new Guest(0, null, name, mail, tel, addr, nat, idType, number, stayDuration, false);
    }

    /// <summary>Maps the document type name shown in the form onto the format rules in <see cref="Input"/>.</summary>
    public static Input.IdDocumentType IdTypeFrom(string? typeName)
    {
        var t = typeName ?? "";
        if (t.Contains("Passport", StringComparison.OrdinalIgnoreCase)) return Input.IdDocumentType.Passport;
        if (t.Contains("National", StringComparison.OrdinalIgnoreCase)) return Input.IdDocumentType.NationalId;
        if (t.Contains("Driver", StringComparison.OrdinalIgnoreCase)) return Input.IdDocumentType.DriverLicense;
        if (t.Contains("UMID", StringComparison.OrdinalIgnoreCase)) return Input.IdDocumentType.IdCard;
        return Input.IdDocumentType.Other;
    }

    /// <summary>Archives the guest. A guest with an open stay or reservation cannot be archived.</summary>
    public void Archive(bool hasOpenReservation)
    {
        if (hasOpenReservation)
            throw new DomainException("This guest has an active stay or upcoming reservation. Cancel or complete it before archiving.");
        IsArchived = true;
    }

    public void Restore() => IsArchived = false;
}

/// <summary>A hotel employee: admin, receptionist or housekeeper.</summary>
public sealed class StaffMember : Person
{
    public StaffMember(int id, string username, string fullName, string email, string phone, string role, bool isActive)
        : base(id, fullName, email, phone)
    {
        Username = username;
        Role = role;
        IsActive = isActive;
    }

    public string Username { get; }
    public string Role { get; }
    public bool IsActive { get; private set; }

    public override string Kind => Role;

    public bool CanManageRooms => Role is Roles.Admin or Roles.Receptionist;

    public void Disable() => IsActive = false;

    public void Enable() => IsActive = true;
}
