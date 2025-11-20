using HotelReservations.Api.Models;

namespace HotelReservations.Api.Data;

public static class SeedData
{
    public static void Initialize(HotelDbContext context)
    {
        if (context.Rooms.Any())
        {
            return; // DB has been seeded
        }

        var rooms = new List<Room>
        {
            new Room { Number = "101", Type = "Single", NightlyRate = 75 },
            new Room { Number = "102", Type = "Single", NightlyRate = 80 },
            new Room { Number = "201", Type = "Double", NightlyRate = 120 },
            new Room { Number = "202", Type = "Double", NightlyRate = 130 },
            new Room { Number = "301", Type = "Suite",  NightlyRate = 220 }
        };

        var guests = new List<Guest>
        {
            new Guest { FullName = "Juan Pérez",   Email = "juan.perez@example.com",   Phone = "+1-809-000-0001" },
            new Guest { FullName = "María García", Email = "maria.garcia@example.com", Phone = "+1-809-000-0002" },
            new Guest { FullName = "Carlos López", Email = "carlos.lopez@example.com", Phone = "+1-809-000-0003" }
        };

        context.Rooms.AddRange(rooms);
        context.Guests.AddRange(guests);

        context.SaveChanges();
    }
}
