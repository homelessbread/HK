using HotelKeeper.Models;
using HotelKeeper.Services;

namespace HotelKeeper.ViewModels;

/// <summary>
/// Builds the sidebar, rail and pages for each role. A role gets only the pages listed here, and the services behind
/// them check the role again, so there is no way to reach another role's screen or data by changing a UI value.
/// </summary>
public static class RoleShells
{
    public static ShellViewModel Create(PageContext ctx, string hotelName) => ctx.Session.Role switch
    {
        Roles.Admin => Admin(ctx, hotelName),
        Roles.Receptionist => Receptionist(ctx, hotelName),
        Roles.Housekeeper => Housekeeper(ctx, hotelName),
        Roles.Guest => Guest(ctx, hotelName),
        _ => throw new Data.AuthorizationException("This account has no interface.")
    };

    static (string key, string title, string icon, Func<PageViewModel> create) Nav(string key, string title, string icon, Func<PageViewModel> create) => (key, title, icon, create);

    static ShellViewModel Admin(PageContext ctx, string name)
    {
        var d = new DashboardService(ctx.Session);
        return new ShellViewModel(ctx, name, new[]
        {
            new NavDefinition("Overview", "home", new[]
            {
                Nav("dashboard", "Dashboard", "layout", () => new DashboardPageViewModel(ctx, "Dashboard", "How the hotel is doing right now.", d.AdminAsync,
                    new[] { new QuickAction("Guest requests", "inbox", "requests"), new QuickAction("Maintenance", "wrench", "maintenance") })),
            }),
            new NavDefinition("Hotel", "bed", new[]
            {
                Nav("rooms", "Rooms", "bed", () => SharedPages.Rooms(ctx, SharedPages.RoomMode.Manage)),
                Nav("guests", "Guests", "users", () => SharedPages.Guests(ctx, "Guests", "Guest accounts and records. Changes are written to the audit log.")),
            }),
            new NavDefinition("Room Operation", "settings", new[]
            {
                Nav("room-types", "Room Types", "tag", () => SharedPages.RoomTypes(ctx)),
                Nav("room-desc", "Room Descriptions", "file-text", () => SharedPages.Rooms(ctx, SharedPages.RoomMode.Manage)),
                Nav("room-pax", "Room Capacity (PAX)", "users", () => SharedPages.RoomTypes(ctx)),
                Nav("room-items", "Room Items", "package", () => SharedPages.RoomItems(ctx)),
                Nav("requestable-items", "Requestable Items", "list", () => StaffPages.ItemCatalog(ctx)),
            }),
            new NavDefinition("System", "settings", new[]
            {
                Nav("staff", "Staffs", "shield", () => StaffPages.Staff(ctx)),
                Nav("maintenance", "Maintenance", "wrench", () => SharedPages.Maintenance(ctx)),
            }),
        });
    }

    static ShellViewModel Receptionist(PageContext ctx, string name)
    {
        var d = new DashboardService(ctx.Session);
        return new ShellViewModel(ctx, name, new[]
        {
            new NavDefinition("Overview", "home", new[]
            {
                Nav("dashboard", "Dashboard", "layout", () => new DashboardPageViewModel(ctx, "Front Desk", "Today's arrivals, departures and rooms.", d.ReceptionistAsync,
                    new[] { new QuickAction("Guest directory", "users", "guests"), new QuickAction("Room status", "bed", "rooms") })),
            }),
            new NavDefinition("Hotel", "bed", new[]
            {
                Nav("rooms", "Rooms", "bed", () => SharedPages.Rooms(ctx, SharedPages.RoomMode.Status, title: "Rooms", description: "Room status and guest occupancy.")),
                Nav("guests", "Guests", "users", () => SharedPages.Guests(ctx, "Guests", "Guest accounts and records.")),
                Nav("housekeeping", "Housekeeping", "sparkles", () => StaffPages.Housekeeping(ctx)),
            }),
            new NavDefinition("System", "settings", new[]
            {
                Nav("account", "Account", "user", () => SharedPages.Account(ctx)),
            }),
        });
    }

    static ShellViewModel Housekeeper(PageContext ctx, string name)
    {
        var d = new DashboardService(ctx.Session);
        return new ShellViewModel(ctx, name, new[]
        {
            new NavDefinition("Overview", "home", new[]
            {
                Nav("dashboard", "Dashboard", "layout", () => new DashboardPageViewModel(ctx, "My day", "Rooms to clean and requests waiting for you.", d.HousekeeperAsync,
                    new[] { new QuickAction("My cleaning tasks", "sparkles", "tasks") })),
            }),
            new NavDefinition("Work", "sparkles", new[]
            {
                Nav("tasks", "Tasks", "sparkles", () => StaffPages.Housekeeping(ctx)),
            }),
            new NavDefinition("Overview", "file-text", new[]
            {
                Nav("report", "Report", "wrench", () => SharedPages.Maintenance(ctx, title: "Report", description: "Report damage or unsafe rooms.")),
            }),
        });
    }

    static ShellViewModel Guest(PageContext ctx, string name)
    {
        var d = new DashboardService(ctx.Session);
        return new ShellViewModel(ctx, name, new[]
        {
            new NavDefinition("Guest", "user", new[]
            {
                Nav("dashboard", "Dashboard", "layout", () => new DashboardPageViewModel(ctx, $"Welcome, {ctx.Session.FullName.Split(' ')[0]}", "Your stay and bookings at a glance.", d.GuestAsync,
                    new[] { new QuickAction("My requests", "inbox", "requests") })),
                Nav("tasks", "Tasks", "list-checks", () => GuestPages.RoomInformation(ctx)),
                Nav("room-info", "Room Info", "key", () => GuestPages.RoomInformation(ctx)),
                Nav("requests", "Requests", "inbox", () => GuestPages.MyRequests(ctx)),
            }),
        });
    }
}
