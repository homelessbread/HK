using HotelKeeper.Models;

namespace HotelKeeper.Services;

/// <summary>
/// Turns the flat rows the services read from the database into domain objects. It lives here, not in Models,
/// so the entities do not depend on how the data is queried.
/// </summary>
public static class EntityMapping
{
    public static Guest ToEntity(this GuestRow r) =>
        new(r.Id, r.UserId, r.FullName, r.Email, r.Phone, r.Address, r.Nationality, r.IdDocumentType, r.IdDocument, r.StayDuration, r.IsArchived);

    public static StaffMember ToEntity(this StaffRow r) =>
        new(r.Id, r.Username, r.FullName, r.Email, r.Phone, r.Role, r.IsActive);

    public static Room ToEntity(this RoomRow r) =>
        new(r.Id, r.RoomNumber, r.Building, r.Floor, r.RoomTypeId, r.RoomType, r.Capacity, r.Rate, r.Status, r.IsArchived);

    public static RoomType ToEntity(this RoomTypeRow r) =>
        new(r.Id, r.Name, r.BaseRate, r.Capacity, r.IsActive);

    public static Reservation ToEntity(this ReservationRow r) =>
        new(r.Id, r.GuestId, r.RoomId, r.CheckInDate, r.CheckOutDate, r.GuestCount, r.Status, r.NightlyRate, r.Notes);

    public static Stay ToEntity(this StayRow r) =>
        new(r.StayId, r.ReservationId, r.GuestId, r.RoomId, r.CheckInAt, r.ExpectedCheckOutDate, r.CheckOutAt);
}
