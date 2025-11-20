namespace HotelReservations.Api.Dtos;

public class CreateReservationDto
{
    public int RoomId { get; set; }
    public int GuestId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public string? Notes { get; set; }
}
