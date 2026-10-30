using HotelKeeper.Data;
using HotelKeeper.Services;
using HotelKeeper.Validation;

namespace HotelKeeper.Models;

/// <summary>A kind of room (Standard, Deluxe...) with its base rate and capacity.</summary>
public sealed class RoomType : Entity
{
    public RoomType(int id, string name, decimal baseRate, int capacity, bool isActive) : base(id)
    {
        Name = name;
        BaseRate = baseRate;
        Capacity = capacity;
        IsActive = isActive;
    }

    public string Name { get; }
    public decimal BaseRate { get; }
    public int Capacity { get; }
    public bool IsActive { get; }
}

/// <summary>A physical room. It owns its status rules through <see cref="RoomState"/>.</summary>
public sealed class Room : Entity, IArchivable
{
    public Room(int id, string roomNumber, string building, int floor, int roomTypeId, string roomTypeName,
        int capacity, decimal rate, string status, bool isArchived) : base(id)
    {
        RoomNumber = roomNumber;
        Building = building;
        Floor = floor;
        RoomTypeId = roomTypeId;
        RoomTypeName = roomTypeName;
        Capacity = capacity;
        Rate = rate;
        Status = status;
        IsArchived = isArchived;
    }

    public string RoomNumber { get; }
    public string Building { get; }
    public int Floor { get; }
    public int RoomTypeId { get; }
    public string RoomTypeName { get; }
    public int Capacity { get; }
    public decimal Rate { get; private set; }
    public string Status { get; }
    public bool IsArchived { get; }

    /// <summary>The state object for the current status.</summary>
    public RoomState State => RoomState.For(Status);

    public string Display => $"Room {RoomNumber} · {RoomTypeName} · floor {Floor}";

    /// <summary>Null when <paramref name="role"/> may manually move this room to <paramref name="target"/>, otherwise the reason not.</summary>
    public string? CheckManualChange(string role, string target) => State.CheckManualChange(role, target);

    public void ChangeRate(decimal rate) =>
        Rate = Validated("Rate", () => Input.Money(rate, 0.01m, 1_000_000m, "Rate"));

    public decimal CostFor(int nights) => Rate * nights;

    /// <summary>Null when the room can be reserved for these guests starting on <paramref name="checkIn"/>, otherwise the reason it cannot.</summary>
    public string? WhyNotReservable(DateTime checkIn, int guests, DateTime today)
    {
        if (IsArchived) return $"Room {RoomNumber} is not available.";
        if (Status == RoomStatus.OutOfService) return $"Room {RoomNumber} is Out of Service and cannot be reserved.";
        if (Capacity < guests) return $"Room {RoomNumber} sleeps {Capacity}; you have {guests} guests.";
        if (checkIn.Date == today.Date && Status != RoomStatus.Available)
            return $"Room {RoomNumber} is {Labels.Of(Status)} and not ready for a same-day arrival.";
        return null;
    }
}

/// <summary>A booking of one room by one guest for a range of nights.</summary>
public sealed class Reservation : Entity
{
    public Reservation(int id, int guestId, int roomId, DateTime checkInDate, DateTime checkOutDate, int guestCount,
        string status, decimal nightlyRate, string notes) : base(id)
    {
        GuestId = guestId;
        RoomId = roomId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        GuestCount = guestCount;
        Status = status;
        NightlyRate = nightlyRate;
        Notes = notes;
    }

    public int GuestId { get; }
    public int RoomId { get; }
    public DateTime CheckInDate { get; }
    public DateTime CheckOutDate { get; }
    public int GuestCount { get; }
    public string Status { get; private set; }
    public decimal NightlyRate { get; }
    public string Notes { get; }

    public int Nights => (CheckOutDate.Date - CheckInDate.Date).Days;
    public decimal Total => NightlyRate * Nights;

    /// <summary>True while the reservation still holds the room.</summary>
    public bool IsOpen => Status is ResStatus.Pending or ResStatus.Confirmed or ResStatus.CheckedIn;

    public bool CanCancel => Status is ResStatus.Pending or ResStatus.Confirmed;

    /// <summary>True when this reservation holds the room during any part of [from, to). Same rule as the availability query.</summary>
    public bool Overlaps(DateTime from, DateTime to) => IsOpen && CheckInDate.Date < to.Date && CheckOutDate.Date > from.Date;

    /// <summary>Creates a new, validated reservation using the same rules as <see cref="ReservationRules"/>.</summary>
    public static Reservation Create(int guestId, int roomId, DateTime checkIn, DateTime checkOut, int guests,
        decimal nightlyRate, string? notes, bool confirmed)
    {
        Input.PositiveId(guestId, "guest");
        Input.PositiveId(roomId, "room");
        ReservationRules.ValidateDates(checkIn, checkOut, guests);
        var cleanNotes = ReservationRules.ValidateNotes(notes);
        return new Reservation(0, guestId, roomId, checkIn.Date, checkOut.Date, guests,
            confirmed ? ResStatus.Confirmed : ResStatus.Pending, nightlyRate, cleanNotes);
    }

    public void Confirm()
    {
        if (Status != ResStatus.Pending)
            throw new DomainException($"Only a Pending reservation can be confirmed (this one is {Labels.Of(Status)}).");
        Status = ResStatus.Confirmed;
    }

    public void Cancel()
    {
        if (!CanCancel)
            throw new DomainException($"A {Labels.Of(Status).ToLower()} reservation cannot be cancelled.");
        Status = ResStatus.Cancelled;
    }
}

/// <summary>A guest's time in a room, from check-in until check-out.</summary>
public sealed class Stay : Entity
{
    public Stay(int id, int reservationId, int guestId, int roomId, DateTime checkInAt, DateTime expectedCheckOutDate,
        DateTime? checkOutAt) : base(id)
    {
        ReservationId = reservationId;
        GuestId = guestId;
        RoomId = roomId;
        CheckInAt = checkInAt;
        ExpectedCheckOutDate = expectedCheckOutDate;
        CheckOutAt = checkOutAt;
    }

    public int ReservationId { get; }
    public int GuestId { get; }
    public int RoomId { get; }
    public DateTime CheckInAt { get; }
    public DateTime ExpectedCheckOutDate { get; }
    public DateTime? CheckOutAt { get; }

    public bool IsActive => CheckOutAt is null;

    /// <summary>True when the guest is still checked in after the day they were due to leave.</summary>
    public bool IsOverdue(DateTime today) => IsActive && ExpectedCheckOutDate.Date < today.Date;
}
