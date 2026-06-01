using SmartParkingSystem.Models;
using SmartParkingSystem.Services;

namespace SmartParkingSystem.Forms;

public class ParkingSimulationForm : Form
{
    private readonly ParkingLot _parkingLot;
    private readonly FlowLayoutPanel _content = new();
    private readonly System.Windows.Forms.Timer _refreshTimer = new();

    public ParkingSimulationForm(ParkingLot parkingLot)
    {
        _parkingLot = parkingLot;
        BuildUi();
        LoadSimulation();
    }

    private void BuildUi()
    {
        Text = "Mo phong hoat dong bai do xe";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 640);
        BackColor = Color.FromArgb(243, 244, 246);
        Font = new Font("Segoe UI", 9.5F);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = Color.FromArgb(17, 24, 39),
            Padding = new Padding(18, 8, 18, 8)
        };
        var title = new Label
        {
            Text = "SO DO BAI DO XE HIEN TAI",
            Dock = DockStyle.Left,
            Width = 500,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };
        var refresh = new Button
        {
            Text = "Lam moi",
            Dock = DockStyle.Right,
            Width = 110,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        refresh.FlatAppearance.BorderSize = 0;
        refresh.Click += (_, _) => LoadSimulation();
        header.Controls.Add(refresh);
        header.Controls.Add(title);
        Controls.Add(header);

        _content.Dock = DockStyle.Fill;
        _content.FlowDirection = FlowDirection.TopDown;
        _content.WrapContents = false;
        _content.AutoScroll = true;
        _content.Padding = new Padding(18);
        Controls.Add(_content);

        _refreshTimer.Interval = 3000;
        _refreshTimer.Tick += (_, _) => LoadSimulation();
        _refreshTimer.Start();
        FormClosed += (_, _) => _refreshTimer.Stop();
    }

    private void LoadSimulation()
    {
        _content.SuspendLayout();
        _content.Controls.Clear();
        var tickets = _parkingLot.SearchActiveTickets(string.Empty);
        var zones = _parkingLot.GetZones();

        foreach (var zone in zones)
        {
            var zoneTickets = tickets
                .Where(ticket => ticket.Vehicle.Type == zone.VehicleType)
                .OrderBy(ticket => ticket.EntryTime)
                .ToList();
            _content.Controls.Add(BuildZonePanel(zone, zoneTickets));
        }

        _content.ResumeLayout();
    }

    private Control BuildZonePanel(ParkingZone zone, List<ParkingTicket> tickets)
    {
        var panel = new Panel
        {
            Width = 820,
            Height = 172,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 14),
            Padding = new Padding(14)
        };

        var title = new Label
        {
            Text = $"{zone.Name} | Dang do: {zone.Occupied}/{zone.Capacity} | Con trong: {zone.Available}",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55)
        };
        panel.Controls.Add(title);

        var grid = new FlowLayoutPanel
        {
            Left = 14,
            Top = 48,
            Width = 790,
            Height = 106,
            WrapContents = true,
            AutoScroll = true
        };

        for (var i = 0; i < zone.Capacity; i++)
        {
            var ticket = i < tickets.Count ? tickets[i] : null;
            grid.Controls.Add(BuildSlot(i + 1, ticket));
        }

        panel.Controls.Add(grid);
        return panel;
    }

    private static Control BuildSlot(int index, ParkingTicket? ticket)
    {
        var occupied = ticket is not null;
        var slot = new Label
        {
            Width = 92,
            Height = 44,
            Margin = new Padding(4),
            BackColor = occupied ? Color.FromArgb(37, 99, 235) : Color.FromArgb(229, 231, 235),
            ForeColor = occupied ? Color.White : Color.FromArgb(75, 85, 99),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Text = occupied ? $"{index:00}\n{ticket!.Vehicle.LicensePlate}" : $"{index:00}\nTRONG"
        };
        return slot;
    }
}
