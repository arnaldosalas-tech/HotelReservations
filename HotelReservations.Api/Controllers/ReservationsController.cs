using HotelReservations.Api.Data;
using HotelReservations.Api.Dtos;
using HotelReservations.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly HotelDbContext _context;

    public ReservationsController(HotelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations()
    {
        var reservations = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .OrderBy(r => r.CheckIn)
            .Select(r => new ReservationDto
            {
                Id = r.Id,
                RoomId = r.RoomId,
                RoomNumber = r.Room != null ? r.Room.Number : string.Empty,
                GuestId = r.GuestId,
                GuestName = r.Guest != null ? r.Guest.FullName : string.Empty,
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                Notes = r.Notes
            })
            .ToListAsync();

        return Ok(reservations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetReservation(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null) return NotFound();

        var dto = new ReservationDto
        {
            Id = reservation.Id,
            RoomId = reservation.RoomId,
            RoomNumber = reservation.Room?.Number ?? string.Empty,
            GuestId = reservation.GuestId,
            GuestName = reservation.Guest?.FullName ?? string.Empty,
            CheckIn = reservation.CheckIn,
            CheckOut = reservation.CheckOut,
            Notes = reservation.Notes
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreateReservationDto dto)
    {
        var validationError = await ValidateReservation(dto.RoomId, dto.GuestId, dto.CheckIn, dto.CheckOut);
        if (!string.IsNullOrEmpty(validationError))
        {
            return BadRequest(validationError);
        }

        var reservation = new Reservation
        {
            RoomId = dto.RoomId,
            GuestId = dto.GuestId,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            Notes = dto.Notes
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var created = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .FirstAsync(r => r.Id == reservation.Id);

        var resultDto = new ReservationDto
        {
            Id = created.Id,
            RoomId = created.RoomId,
            RoomNumber = created.Room?.Number ?? string.Empty,
            GuestId = created.GuestId,
            GuestName = created.Guest?.FullName ?? string.Empty,
            CheckIn = created.CheckIn,
            CheckOut = created.CheckOut,
            Notes = created.Notes
        };

        return CreatedAtAction(nameof(GetReservation), new { id = created.Id }, resultDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ReservationDto>> UpdateReservation(int id, [FromBody] UpdateReservationDto dto)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound("Reserva no encontrada.");

        var validationError = await ValidateReservation(dto.RoomId, dto.GuestId, dto.CheckIn, dto.CheckOut, id);
        if (!string.IsNullOrEmpty(validationError))
        {
            return BadRequest(validationError);
        }

        reservation.RoomId = dto.RoomId;
        reservation.GuestId = dto.GuestId;
        reservation.CheckIn = dto.CheckIn;
        reservation.CheckOut = dto.CheckOut;
        reservation.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        var updated = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .FirstAsync(r => r.Id == reservation.Id);

        var resultDto = new ReservationDto
        {
            Id = updated.Id,
            RoomId = updated.RoomId,
            RoomNumber = updated.Room?.Number ?? string.Empty,
            GuestId = updated.GuestId,
            GuestName = updated.Guest?.FullName ?? string.Empty,
            CheckIn = updated.CheckIn,
            CheckOut = updated.CheckOut,
            Notes = updated.Notes
        };

        return Ok(resultDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound("Reserva no encontrada.");

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string?> ValidateReservation(int roomId, int guestId, DateTime checkIn, DateTime checkOut, int? reservationId = null)
    {
        if (checkOut <= checkIn)
        {
            return "La fecha de salida debe ser mayor que la fecha de entrada.";
        }

        if (checkIn.Date < DateTime.Today)
        {
            return "La fecha de entrada no puede ser en el pasado.";
        }

        var roomExists = await _context.Rooms.AnyAsync(r => r.Id == roomId);
        if (!roomExists)
        {
            return "La habitación seleccionada no existe.";
        }

        var guestExists = await _context.Guests.AnyAsync(g => g.Id == guestId);
        if (!guestExists)
        {
            return "El huésped seleccionado no existe.";
        }

        var overlapping = await _context.Reservations
            .Where(r => r.RoomId == roomId && (!reservationId.HasValue || r.Id != reservationId.Value))
            .Where(r => checkIn < r.CheckOut && checkOut > r.CheckIn)
            .AnyAsync();

        if (overlapping)
        {
            return "La habitación ya está reservada en el rango de fechas seleccionado.";
        }

        return null;
    }
}
