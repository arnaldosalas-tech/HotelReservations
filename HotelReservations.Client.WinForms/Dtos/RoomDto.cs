namespace HotelReservations.Client.WinForms.Dtos;

public class RoomDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal NightlyRate { get; set; }
}
