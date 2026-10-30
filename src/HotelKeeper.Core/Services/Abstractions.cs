namespace HotelKeeper.Services;

/// <summary>What the rest of the program may ask of guest records. The UI can depend on this instead of the concrete class.</summary>
public interface IGuestService
{
    Task<GuestCreated> RegisterAsync(string fullName, string email, string phone, string address, string nationality,
        string idDocumentType, string idDocument, int stayDuration, string? username = null);

    Task<List<GuestRow>> SearchAsync(string? search = null, bool archived = false, bool inHouseOnly = false);

    Task<GuestRow?> GetAsync(int guestId);

    Task<string> ResetPasswordAsync(int guestId);

    Task SetArchivedAsync(int guestId, bool archived);
}

/// <summary>What the rest of the program may ask of rooms.</summary>
public interface IRoomService
{
    Task<List<RoomRow>> ListAsync(string? status = null, string? search = null, bool archived = false);

    Task<Dictionary<string, int>> CountsAsync();

    Task<List<RoomOffer>> AvailableAsync(DateTime checkIn, DateTime checkOut, int guests, int? roomTypeId = null, int? ignoreReservationId = null);

    Task ChangeStatusAsync(int roomId, string target, string? reason);
}
