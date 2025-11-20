namespace HotelReservations.Api.Models;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Single, Double, Suite
    public decimal NightlyRate { get; set; }
}
