using HotelReservations.Api.Data;
using HotelReservations.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly HotelDbContext _context;

    public RoomsController(HotelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
    {
        var rooms = await _context.Rooms
            .OrderBy(r => r.Number)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                Number = r.Number,
                Type = r.Type,
                NightlyRate = r.NightlyRate
            })
            .ToListAsync();

        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> GetRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        var dto = new RoomDto
        {
            Id = room.Id,
            Number = room.Number,
            Type = room.Type,
            NightlyRate = room.NightlyRate
        };
        return Ok(dto);
    }
}
