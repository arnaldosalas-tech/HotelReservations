using HotelReservations.Api.Data;
using HotelReservations.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestsController : ControllerBase
{
    private readonly HotelDbContext _context;

    public GuestsController(HotelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestDto>>> GetGuests()
    {
        var guests = await _context.Guests
            .OrderBy(g => g.FullName)
            .Select(g => new GuestDto
            {
                Id = g.Id,
                FullName = g.FullName,
                Email = g.Email
            })
            .ToListAsync();

        return Ok(guests);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GuestDto>> GetGuest(int id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest == null) return NotFound();

        var dto = new GuestDto
        {
            Id = guest.Id,
            FullName = guest.FullName,
            Email = guest.Email
        };
        return Ok(dto);
    }
}
