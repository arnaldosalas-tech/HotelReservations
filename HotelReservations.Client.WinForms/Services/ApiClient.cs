using System.Net.Http.Json;
using HotelReservations.Client.WinForms.Dtos;

namespace HotelReservations.Client.WinForms.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<List<ReservationDto>> GetReservationsAsync()
    {
        var response = await _httpClient.GetAsync("api/reservations");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al obtener reservas: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var data = await response.Content.ReadFromJsonAsync<List<ReservationDto>>();
        return data ?? new List<ReservationDto>();
    }

    public async Task<List<RoomDto>> GetRoomsAsync()
    {
        var response = await _httpClient.GetAsync("api/rooms");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al obtener habitaciones: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var data = await response.Content.ReadFromJsonAsync<List<RoomDto>>();
        return data ?? new List<RoomDto>();
    }

    public async Task<List<GuestDto>> GetGuestsAsync()
    {
        var response = await _httpClient.GetAsync("api/guests");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al obtener huéspedes: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var data = await response.Content.ReadFromJsonAsync<List<GuestDto>>();
        return data ?? new List<GuestDto>();
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reservations", dto);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error de validación (400): {error}");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al crear reserva: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var created = await response.Content.ReadFromJsonAsync<ReservationDto>();
        return created ?? throw new Exception("No se pudo leer la reserva creada.");
    }

    public async Task<ReservationDto> UpdateReservationAsync(int id, UpdateReservationDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/reservations/{id}", dto);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new Exception("Reserva no encontrada (404).");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error de validación (400): {error}");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al actualizar reserva: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var updated = await response.Content.ReadFromJsonAsync<ReservationDto>();
        return updated ?? throw new Exception("No se pudo leer la reserva actualizada.");
    }

    public async Task DeleteReservationAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/reservations/{id}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new Exception("Reserva no encontrada (404).");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error al eliminar reserva: {(int)response.StatusCode} {response.ReasonPhrase}");
        }
    }
}
