namespace HotelKeeper.Models;

/// <summary>
/// One state a room can be in. Each state answers for itself which manual changes it allows, so adding a
/// state means adding a class, not editing a long switch. System-driven changes (check-in, check-out,
/// housekeeping) do not go through here; they have their own code paths.
/// </summary>
public abstract class RoomState
{
    /// <summary>The status text stored in the database, e.g. "Available".</summary>
    public abstract string Name { get; }

    /// <summary>Returns null when <paramref name="role"/> may manually change the room to <paramref name="target"/>, otherwise the reason it may not.</summary>
    public string? CheckManualChange(string role, string target)
    {
        if (Name == target) return $"Room is already {Labels.Of(target)}.";
        return Evaluate(role, target);
    }

    /// <summary>The state-specific rule. Called only when the target differs from this state.</summary>
    protected abstract string? Evaluate(string role, string target);

    static readonly RoomState available = new AvailableRoomState();
    static readonly RoomState occupied = new OccupiedRoomState();
    static readonly RoomState dirty = new DirtyRoomState();
    static readonly RoomState cleaning = new CleaningRoomState();
    static readonly RoomState outOfService = new OutOfServiceRoomState();

    /// <summary>Finds the state object for a status text. An unrecognised text gets a state that allows no manual change.</summary>
    public static RoomState For(string? status) => status switch
    {
        RoomStatus.Available => available,
        RoomStatus.Occupied => occupied,
        RoomStatus.Dirty => dirty,
        RoomStatus.Cleaning => cleaning,
        RoomStatus.OutOfService => outOfService,
        _ => new UnknownRoomState(status ?? "")
    };

    static bool IsFrontDesk(string role) => role is Roles.Receptionist or Roles.Admin;

    /// <summary>Why a target status can only be reached by the system, not by hand.</summary>
    static string RefuseSystemOnly(string target) => target switch
    {
        RoomStatus.Occupied => "A room becomes Occupied only through check-in.",
        RoomStatus.Cleaning => "A room moves to Cleaning when a housekeeper starts its cleaning task.",
        RoomStatus.Available => "A room becomes Available only when housekeeping completes its cleaning task.",
        _ => "That status change is not allowed."
    };

    /// <summary>Front desk and admins may take a room out of service; every other manual change is refused.</summary>
    static string? AllowOutOfService(string role, string target) =>
        target == RoomStatus.OutOfService && IsFrontDesk(role) ? null : RefuseSystemOnly(target);

    sealed class AvailableRoomState : RoomState
    {
        public override string Name => RoomStatus.Available;

        protected override string? Evaluate(string role, string target) =>
            target == RoomStatus.Dirty && IsFrontDesk(role) ? null : AllowOutOfService(role, target);
    }

    sealed class DirtyRoomState : RoomState
    {
        public override string Name => RoomStatus.Dirty;
        protected override string? Evaluate(string role, string target) => AllowOutOfService(role, target);
    }

    sealed class CleaningRoomState : RoomState
    {
        public override string Name => RoomStatus.Cleaning;
        protected override string? Evaluate(string role, string target) => AllowOutOfService(role, target);
    }

    sealed class OccupiedRoomState : RoomState
    {
        public override string Name => RoomStatus.Occupied;
        protected override string? Evaluate(string role, string target) => "An occupied room changes status only through check-out.";
    }

    sealed class OutOfServiceRoomState : RoomState
    {
        public override string Name => RoomStatus.OutOfService;

        protected override string? Evaluate(string role, string target) =>
            target == RoomStatus.Dirty && role == Roles.Admin
                ? null
                : "Only an admin can clear a room that is Out of Service, and it returns as Dirty so it is cleaned first.";
    }

    sealed class UnknownRoomState(string name) : RoomState
    {
        public override string Name => name;
        protected override string? Evaluate(string role, string target) => RefuseSystemOnly(target);
    }
}
