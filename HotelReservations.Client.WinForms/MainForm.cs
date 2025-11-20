using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HotelReservations.Client.WinForms.Dtos;
using HotelReservations.Client.WinForms.Services;

namespace HotelReservations.Client.WinForms;

public class MainForm : Form
{
    private readonly ApiClient _apiClient;

    private DataGridView _dgvReservations = null!;
    private ComboBox _cmbRooms = null!;
    private ComboBox _cmbGuests = null!;
    private DateTimePicker _dtpCheckIn = null!;
    private DateTimePicker _dtpCheckOut = null!;
    private TextBox _txtNotes = null!;
    private Button _btnLoad = null!;
    private Button _btnCreate = null!;
    private Button _btnUpdate = null!;
    private Button _btnDelete = null!;
    private Label _lblStatus = null!;

    private List<RoomDto> _rooms = new();
    private List<GuestDto> _guests = new();
    private List<ReservationDto> _reservations = new();

    public MainForm()
    {
        Text = "Gestión de Reservas de Hotel - Cliente";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        // Cambia la URL si usas otro puerto
        _apiClient = new ApiClient("http://localhost:5000/");

        InitializeControls();
        Load += async (_, _) => await InitializeDataAsync();
    }

    private void InitializeControls()
    {
        _dgvReservations = new DataGridView
        {
            Dock = DockStyle.Top,
            Height = 260,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        _dgvReservations.SelectionChanged += DgvReservations_SelectionChanged;
        Controls.Add(_dgvReservations);

        var panel = new Panel
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(panel);

        var lblRoom = new Label { Text = "Habitación:", Left = 10, Top = 10, AutoSize = true };
        _cmbRooms = new ComboBox { Left = 100, Top = 8, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblGuest = new Label { Text = "Huésped:", Left = 10, Top = 45, AutoSize = true };
        _cmbGuests = new ComboBox { Left = 100, Top = 43, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblCheckIn = new Label { Text = "Check-In:", Left = 10, Top = 80, AutoSize = true };
        _dtpCheckIn = new DateTimePicker { Left = 100, Top = 78, Width = 200, Format = DateTimePickerFormat.Short };

        var lblCheckOut = new Label { Text = "Check-Out:", Left = 10, Top = 115, AutoSize = true };
        _dtpCheckOut = new DateTimePicker { Left = 100, Top = 113, Width = 200, Format = DateTimePickerFormat.Short };

        var lblNotes = new Label { Text = "Notas:", Left = 10, Top = 150, AutoSize = true };
        _txtNotes = new TextBox { Left = 100, Top = 148, Width = 350, Height = 60, Multiline = true };

        _btnLoad = new Button { Text = "Recargar", Left = 500, Top = 10, Width = 120, Height = 30 };
        _btnCreate = new Button { Text = "Crear", Left = 500, Top = 50, Width = 120, Height = 30 };
        _btnUpdate = new Button { Text = "Actualizar", Left = 500, Top = 90, Width = 120, Height = 30 };
        _btnDelete = new Button { Text = "Eliminar", Left = 500, Top = 130, Width = 120, Height = 30 };

        _btnLoad.Click += async (_, _) => await LoadReservationsAsync();
        _btnCreate.Click += async (_, _) => await CreateReservationAsync();
        _btnUpdate.Click += async (_, _) => await UpdateReservationAsync();
        _btnDelete.Click += async (_, _) => await DeleteReservationAsync();

        _lblStatus = new Label { Left = 10, Top = 230, Width = 800, AutoSize = false };

        panel.Controls.AddRange(new Control[]
        {
            lblRoom, _cmbRooms,
            lblGuest, _cmbGuests,
            lblCheckIn, _dtpCheckIn,
            lblCheckOut, _dtpCheckOut,
            lblNotes, _txtNotes,
            _btnLoad, _btnCreate, _btnUpdate, _btnDelete,
            _lblStatus
        });
    }

    private async Task InitializeDataAsync()
    {
        await LoadRoomsAndGuestsAsync();
        await LoadReservationsAsync();
    }

    private async Task LoadRoomsAndGuestsAsync()
    {
        try
        {
            _rooms = await _apiClient.GetRoomsAsync();
            _guests = await _apiClient.GetGuestsAsync();

            _cmbRooms.DataSource = _rooms;
            _cmbRooms.DisplayMember = "Number";
            _cmbRooms.ValueMember = "Id";

            _cmbGuests.DataSource = _guests;
            _cmbGuests.DisplayMember = "FullName";
            _cmbGuests.ValueMember = "Id";

            _dtpCheckIn.Value = DateTime.Today.AddDays(1);
            _dtpCheckOut.Value = DateTime.Today.AddDays(2);
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al cargar habitaciones/huéspedes: {ex.Message}", true);
        }
    }

    private async Task LoadReservationsAsync()
    {
        try
        {
            _reservations = await _apiClient.GetReservationsAsync();
            _dgvReservations.DataSource = _reservations
                .Select(r => new
                {
                    r.Id,
                    Habitación = r.RoomNumber,
                    Huésped = r.GuestName,
                    CheckIn = r.CheckIn.ToShortDateString(),
                    CheckOut = r.CheckOut.ToShortDateString(),
                    r.Notes
                })
                .ToList();

            ShowStatus($"Reservas cargadas: {_reservations.Count}");
        }
        catch (Exception ex)
        {
            ShowStatus($"Error al cargar reservas: {ex.Message}", true);
        }
    }

    private async Task CreateReservationAsync()
    {
        try
        {
            if (_cmbRooms.SelectedItem == null || _cmbGuests.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar una habitación y un huésped.");
                return;
            }

            var roomId = ((RoomDto)_cmbRooms.SelectedItem).Id;
            var guestId = ((GuestDto)_cmbGuests.SelectedItem).Id;
            var checkIn = _dtpCheckIn.Value.Date;
            var checkOut = _dtpCheckOut.Value.Date;

            if (checkOut <= checkIn)
            {
                MessageBox.Show("La fecha de salida debe ser mayor que la fecha de entrada.");
                return;
            }

            var dto = new CreateReservationDto
            {
                RoomId = roomId,
                GuestId = guestId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Notes = string.IsNullOrWhiteSpace(_txtNotes.Text) ? null : _txtNotes.Text.Trim()
            };

            var created = await _apiClient.CreateReservationAsync(dto);
            MessageBox.Show($"Reserva creada con Id {created.Id}", "Éxito");

            await LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al crear reserva: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task UpdateReservationAsync()
    {
        if (_dgvReservations.CurrentRow == null)
        {
            MessageBox.Show("Debe seleccionar una reserva.");
            return;
        }

        try
        {
            var selectedId = (int)_dgvReservations.CurrentRow.Cells["Id"].Value;

            var roomId = ((RoomDto)_cmbRooms.SelectedItem!).Id;
            var guestId = ((GuestDto)_cmbGuests.SelectedItem!).Id;
            var checkIn = _dtpCheckIn.Value.Date;
            var checkOut = _dtpCheckOut.Value.Date;

            if (checkOut <= checkIn)
            {
                MessageBox.Show("La fecha de salida debe ser mayor que la fecha de entrada.");
                return;
            }

            var dto = new UpdateReservationDto
            {
                RoomId = roomId,
                GuestId = guestId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Notes = string.IsNullOrWhiteSpace(_txtNotes.Text) ? null : _txtNotes.Text.Trim()
            };

            await _apiClient.UpdateReservationAsync(selectedId, dto);
            MessageBox.Show("Reserva actualizada.", "Éxito");
            await LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar reserva: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteReservationAsync()
    {
        if (_dgvReservations.CurrentRow == null)
        {
            MessageBox.Show("Debe seleccionar una reserva.");
            return;
        }

        var selectedId = (int)_dgvReservations.CurrentRow.Cells["Id"].Value;
        var confirm = MessageBox.Show("¿Seguro que desea eliminar esta reserva?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        try
        {
            await _apiClient.DeleteReservationAsync(selectedId);
            MessageBox.Show("Reserva eliminada.", "Éxito");
            await LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar reserva: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvReservations_SelectionChanged(object? sender, EventArgs e)
    {
        if (_dgvReservations.CurrentRow == null || _reservations.Count == 0)
            return;

        try
        {
            var selectedId = (int)_dgvReservations.CurrentRow.Cells["Id"].Value;
            var res = _reservations.FirstOrDefault(r => r.Id == selectedId);
            if (res == null) return;

            var room = _rooms.FirstOrDefault(r => r.Id == res.RoomId);
            if (room != null) _cmbRooms.SelectedItem = room;

            var guest = _guests.FirstOrDefault(g => g.Id == res.GuestId);
            if (guest != null) _cmbGuests.SelectedItem = guest;

            _dtpCheckIn.Value = res.CheckIn;
            _dtpCheckOut.Value = res.CheckOut;
            _txtNotes.Text = res.Notes ?? string.Empty;
        }
        catch
        {
            // ignore selection errors
        }
    }

    private void ShowStatus(string message, bool isError = false)
    {
        _lblStatus.Text = message;
        _lblStatus.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.DarkGreen;
    }
}
