using SmartParkingSystem.Models;
using SmartParkingSystem.Services;

namespace SmartParkingSystem.Forms;

public class MainForm : Form
{
    private readonly User _currentUser;
    private readonly ParkingLot _parkingLot;
    private readonly StatisticsService _statisticsService;

    private readonly TextBox _txtEntryPlate = new();
    private readonly ComboBox _cboVehicleType = new();
    private readonly DateTimePicker _dtEntry = new();
    private readonly TextBox _txtExitPlate = new();
    private readonly TextBox _txtSearch = new();
    private readonly DataGridView _grid = new();
    private readonly Label _lblTotalSlots = new();
    private readonly Label _lblCurrentVehicles = new();
    private readonly Label _lblAvailableSlots = new();
    private readonly Label _lblRevenueToday = new();
    private readonly Label _lblRevenueWeek = new();
    private readonly Label _lblRevenueMonth = new();
    private readonly Panel _barrierArm = new();
    private readonly Label _lblBarrier = new();
    private readonly System.Windows.Forms.Timer _barrierTimer = new();
    private int _barrierStep;

    public MainForm(User currentUser, ParkingLot parkingLot, StatisticsService statisticsService)
    {
        _currentUser = currentUser;
        _parkingLot = parkingLot;
        _statisticsService = statisticsService;
        BuildUi();
        LoadDashboard();
    }

    private void BuildUi()
    {
        Text = "Smart Parking System";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1160, 720);
        BackColor = Color.FromArgb(243, 244, 246);
        Font = new Font("Segoe UI", 9.5F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 172));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildStatsPanel(), 0, 1);
        root.Controls.Add(BuildWorkspace(), 0, 2);

        _barrierTimer.Interval = 55;
        _barrierTimer.Tick += BarrierTimerTick;
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(17, 24, 39), Padding = new Padding(18, 8, 18, 8) };
        var title = new Label
        {
            Text = "BAI GUI XE THONG MINH",
            Dock = DockStyle.Left,
            Width = 420,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold)
        };
        var user = new Label
        {
            Text = $"{_currentUser.FullName} - {_currentUser.Role}",
            Dock = DockStyle.Right,
            Width = 280,
            ForeColor = Color.FromArgb(209, 213, 219),
            TextAlign = ContentAlignment.MiddleRight
        };
        header.Controls.Add(user);
        header.Controls.Add(title);
        return header;
    }

    private Control BuildStatsPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Padding = new Padding(0, 14, 0, 14)
        };
        for (var i = 0; i < 6; i++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
        }

        layout.Controls.Add(MakeStatCard("Tong so cho", _lblTotalSlots, Color.FromArgb(59, 130, 246)), 0, 0);
        layout.Controls.Add(MakeStatCard("Xe hien tai", _lblCurrentVehicles, Color.FromArgb(245, 158, 11)), 1, 0);
        layout.Controls.Add(MakeStatCard("Con trong", _lblAvailableSlots, Color.FromArgb(16, 185, 129)), 2, 0);
        layout.Controls.Add(MakeStatCard("Doanh thu ngay", _lblRevenueToday, Color.FromArgb(99, 102, 241)), 3, 0);
        layout.Controls.Add(MakeStatCard("Doanh thu tuan", _lblRevenueWeek, Color.FromArgb(14, 165, 233)), 4, 0);
        layout.Controls.Add(MakeStatCard("Doanh thu thang", _lblRevenueMonth, Color.FromArgb(244, 63, 94)), 5, 0);
        return layout;
    }

    private static Panel MakeStatCard(string title, Label valueLabel, Color accent)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(4),
            Padding = new Padding(14)
        };
        var bar = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(75, 85, 99)
        };
        valueLabel.Text = "0";
        valueLabel.Dock = DockStyle.Fill;
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        valueLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        valueLabel.ForeColor = Color.FromArgb(31, 41, 55);
        panel.Controls.Add(valueLabel);
        panel.Controls.Add(titleLabel);
        panel.Controls.Add(bar);
        return panel;
    }

    private Control BuildWorkspace()
    {
        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        workspace.Controls.Add(BuildLeftPanel(), 0, 0);
        workspace.Controls.Add(BuildGridPanel(), 1, 0);
        return workspace;
    }

    private Control BuildLeftPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 0, 12, 0)
        };
        panel.Controls.Add(BuildEntryBox());
        panel.Controls.Add(BuildExitBox());
        panel.Controls.Add(BuildBarrierBox());
        return panel;
    }

    private Control BuildEntryBox()
    {
        var group = MakeGroup("Xe vao", 350, 224);
        ConfigureTextBox(_txtEntryPlate, "Nhap bien so");
        ConfigureVehicleType();
        _dtEntry.Format = DateTimePickerFormat.Custom;
        _dtEntry.CustomFormat = "dd/MM/yyyy HH:mm";
        _dtEntry.Value = DateTime.Now;
        _dtEntry.Width = 300;

        var btn = MakePrimaryButton("Ghi nhan xe vao");
        btn.Click += CheckInClick;

        group.Controls.Add(MakeLabel("Bien so"));
        group.Controls.Add(_txtEntryPlate);
        group.Controls.Add(MakeLabel("Loai xe"));
        group.Controls.Add(_cboVehicleType);
        group.Controls.Add(MakeLabel("Thoi gian vao"));
        group.Controls.Add(_dtEntry);
        group.Controls.Add(btn);
        return group;
    }

    private Control BuildExitBox()
    {
        var group = MakeGroup("Xe ra", 350, 160);
        ConfigureTextBox(_txtExitPlate, "Nhap bien so can tim");
        var btnPreview = MakeSecondaryButton("Tim va tinh phi");
        btnPreview.Click += PreviewExitClick;
        var btnPay = MakePrimaryButton("Thanh toan - cho xe ra");
        btnPay.Click += CheckOutClick;
        group.Controls.Add(MakeLabel("Bien so"));
        group.Controls.Add(_txtExitPlate);
        group.Controls.Add(btnPreview);
        group.Controls.Add(btnPay);
        return group;
    }

    private Control BuildBarrierBox()
    {
        var group = MakeGroup("Mo phong barie", 350, 142);
        _lblBarrier.Text = "DONG";
        _lblBarrier.Height = 26;
        _lblBarrier.Width = 300;
        _lblBarrier.TextAlign = ContentAlignment.MiddleCenter;
        _lblBarrier.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _lblBarrier.ForeColor = Color.FromArgb(185, 28, 28);

        var scene = new Panel { Width = 300, Height = 72, BackColor = Color.FromArgb(229, 231, 235) };
        var pole = new Panel { Width = 16, Height = 54, Left = 26, Top = 10, BackColor = Color.FromArgb(55, 65, 81) };
        _barrierArm.Width = 210;
        _barrierArm.Height = 10;
        _barrierArm.Left = 42;
        _barrierArm.Top = 22;
        _barrierArm.BackColor = Color.FromArgb(220, 38, 38);
        scene.Controls.Add(_barrierArm);
        scene.Controls.Add(pole);
        group.Controls.Add(_lblBarrier);
        group.Controls.Add(scene);
        return group;
    }

    private Control BuildGridPanel()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var searchPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 0, 8) };
        ConfigureTextBox(_txtSearch, "Tim kiem bien so");
        _txtSearch.Width = 280;
        _txtSearch.Left = 0;
        _txtSearch.Top = 8;
        _txtSearch.TextChanged += (_, _) => LoadGrid();
        var btnRefresh = MakeSecondaryButton("Lam moi");
        btnRefresh.Left = 296;
        btnRefresh.Top = 8;
        btnRefresh.Width = 110;
        btnRefresh.Click += (_, _) => LoadDashboard();
        searchPanel.Controls.Add(_txtSearch);
        searchPanel.Controls.Add(btnRefresh);

        ConfigureGrid();
        panel.Controls.Add(searchPanel, 0, 0);
        panel.Controls.Add(_grid, 0, 1);
        return panel;
    }

    private void CheckInClick(object? sender, EventArgs e)
    {
        try
        {
            var plate = RequirePlate(_txtEntryPlate.Text);
            var type = (VehicleType)_cboVehicleType.SelectedItem!;
            _parkingLot.CheckIn(plate, type, _dtEntry.Value, _currentUser.Id);
            AnimateBarrier();
            MessageBox.Show("Da ghi nhan xe vao bai.", "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _txtEntryPlate.Clear();
            _dtEntry.Value = DateTime.Now;
            LoadDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Khong the cho xe vao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void PreviewExitClick(object? sender, EventArgs e)
    {
        try
        {
            var plate = RequirePlate(_txtExitPlate.Text);
            var (ticket, fee) = _parkingLot.PreviewExit(plate, DateTime.Now);
            MessageBox.Show(
                $"Bien so: {ticket.Vehicle.LicensePlate}\nLoai xe: {ticket.Vehicle.Type}\nVao luc: {ticket.EntryTime:dd/MM/yyyy HH:mm}\nThoi gian gui: {FormatDuration(ticket.Duration)}\nPhi tam tinh: {fee:N0} VND",
                "Thong tin xe ra",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Khong tim thay", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CheckOutClick(object? sender, EventArgs e)
    {
        try
        {
            var plate = RequirePlate(_txtExitPlate.Text);
            var exitTime = DateTime.Now;
            var (ticket, fee) = _parkingLot.PreviewExit(plate, exitTime);
            var confirm = MessageBox.Show(
                $"Thanh toan {fee:N0} VND cho xe {ticket.Vehicle.LicensePlate}?",
                "Xac nhan thanh toan",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            _parkingLot.CheckOut(plate, exitTime, "Cash");
            AnimateBarrier();
            MessageBox.Show("Da thanh toan va ghi lich su giao dich.", "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _txtExitPlate.Clear();
            LoadDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Khong the cho xe ra", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadDashboard()
    {
        var stats = _statisticsService.GetDashboardStats();
        _lblTotalSlots.Text = stats.TotalSlots.ToString("N0");
        _lblCurrentVehicles.Text = stats.CurrentVehicles.ToString("N0");
        _lblAvailableSlots.Text = stats.AvailableSlots.ToString("N0");
        _lblRevenueToday.Text = $"{stats.RevenueToday:N0} VND";
        _lblRevenueWeek.Text = $"{stats.RevenueThisWeek:N0} VND";
        _lblRevenueMonth.Text = $"{stats.RevenueThisMonth:N0} VND";
        LoadGrid();
    }

    private void LoadGrid()
    {
        var rows = _parkingLot.SearchActiveTickets(_txtSearch.Text)
            .Select(t => new
            {
                MaVe = t.Id,
                BienSo = t.Vehicle.LicensePlate,
                LoaiXe = t.Vehicle.Type,
                ThoiGianVao = t.EntryTime.ToString("dd/MM/yyyy HH:mm"),
                ThoiGianGui = FormatDuration(t.Duration)
            })
            .ToList();
        _grid.DataSource = rows;
    }

    private void AnimateBarrier()
    {
        _barrierStep = 0;
        _barrierTimer.Start();
    }

    private void BarrierTimerTick(object? sender, EventArgs e)
    {
        _barrierStep++;
        var opening = _barrierStep <= 12;
        _barrierArm.Top = opening ? 22 - _barrierStep : 10 + (_barrierStep - 12);
        _barrierArm.Width = opening ? 210 - _barrierStep * 8 : 114 + (_barrierStep - 12) * 8;
        _barrierArm.BackColor = opening ? Color.FromArgb(34, 197, 94) : Color.FromArgb(220, 38, 38);
        _lblBarrier.Text = opening ? "DANG MO" : "DANG DONG";
        _lblBarrier.ForeColor = opening ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
        if (_barrierStep >= 24)
        {
            _barrierTimer.Stop();
            _barrierArm.Top = 22;
            _barrierArm.Width = 210;
            _barrierArm.BackColor = Color.FromArgb(220, 38, 38);
            _lblBarrier.Text = "DONG";
        }
    }

    private static FlowLayoutPanel MakeGroup(string title, int width, int height)
    {
        var group = new FlowLayoutPanel
        {
            Width = width,
            Height = height,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 12)
        };
        group.Controls.Add(new Label
        {
            Text = title,
            Width = width - 42,
            Height = 28,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55)
        });
        return group;
    }

    private static Label MakeLabel(string text)
    {
        return new Label
        {
            Text = text,
            Width = 300,
            Height = 22,
            ForeColor = Color.FromArgb(75, 85, 99)
        };
    }

    private static Button MakePrimaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Width = 300,
            Height = 36,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Button MakeSecondaryButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Width = 300,
            Height = 34,
            BackColor = Color.FromArgb(229, 231, 235),
            ForeColor = Color.FromArgb(31, 41, 55),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 8, 0, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static void ConfigureTextBox(TextBox textBox, string placeholder)
    {
        textBox.Width = 300;
        textBox.Height = 30;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.PlaceholderText = placeholder;
        textBox.CharacterCasing = CharacterCasing.Upper;
    }

    private void ConfigureVehicleType()
    {
        _cboVehicleType.Width = 300;
        _cboVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboVehicleType.DataSource = Enum.GetValues<VehicleType>();
        _cboVehicleType.SelectedItem = VehicleType.Motorbike;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 41, 55);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(191, 219, 254);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
    }

    private static string RequirePlate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new InvalidOperationException("Vui long nhap bien so xe.");
        }
        return input.Trim().ToUpperInvariant();
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
        {
            return "Duoi 1 phut";
        }

        return $"{(int)duration.TotalHours} gio {duration.Minutes} phut";
    }
}
