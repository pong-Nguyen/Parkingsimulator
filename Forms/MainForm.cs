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
    private readonly Label _lblRecognitionStatus = new();
    private readonly Label _lblTotalSlots = new();
    private readonly Label _lblCurrentVehicles = new();
    private readonly Label _lblAvailableSlots = new();
    private readonly Label _lblRevenueToday = new();
    private readonly Label _lblRevenueWeek = new();
    private readonly Label _lblRevenueMonth = new();
    private readonly Panel _barrierArm = new();
    private readonly Label _lblBarrier = new();
    private readonly System.Windows.Forms.Timer _barrierTimer = new();
    private readonly System.Windows.Forms.Timer _autoDetectTimer = new();
    private readonly Random _random = new();
    private readonly CheckBox _chkAutoDetect = new();
    private readonly Dictionary<VehicleType, NumericUpDown> _firstHourInputs = new();
    private readonly Dictionary<VehicleType, NumericUpDown> _nextHourInputs = new();
    private readonly Dictionary<VehicleType, NumericUpDown> _zoneCapacityInputs = new();
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

        _autoDetectTimer.Interval = 5000;
        _autoDetectTimer.Tick += (_, _) => DetectAndCheckInVehicle(showPopup: false);
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(17, 24, 39), Padding = new Padding(18, 8, 18, 8) };
        var title = new Label
        {
            Text = "BAI GUI XE THONG MINH",
            Dock = DockStyle.Left,
            Width = 520,
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
        var simulationButton = new Button
        {
            Text = "So do bai xe",
            Dock = DockStyle.Right,
            Width = 130,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };
        simulationButton.FlatAppearance.BorderSize = 0;
        simulationButton.Click += (_, _) => new ParkingSimulationForm(_parkingLot).Show(this);
        header.Controls.Add(user);
        header.Controls.Add(simulationButton);
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
        panel.Controls.Add(BuildPricingBox());
        panel.Controls.Add(BuildZoneConfigBox());
        panel.Controls.Add(BuildBarrierBox());
        return panel;
    }

    private Control BuildEntryBox()
    {
        var group = MakeGroup("Xe vao - nhan dien tu dong", 350, 430);
        ConfigureTextBox(_txtEntryPlate, "Nhap bien so");
        ConfigureVehicleType();
        _dtEntry.Format = DateTimePickerFormat.Custom;
        _dtEntry.CustomFormat = "dd/MM/yyyy HH:mm";
        _dtEntry.Value = DateTime.Now;
        _dtEntry.Width = 300;

        var cameraPanel = BuildCameraPanel();
        var btnDetect = MakePrimaryButton("Quet xe vao");
        btnDetect.Click += (_, _) => DetectAndCheckInVehicle(showPopup: true);
        var btnManual = MakeSecondaryButton("Ghi nhan thu cong");
        btnManual.Click += CheckInClick;

        _chkAutoDetect.Text = "Tu dong quet moi 5 giay";
        _chkAutoDetect.Width = 300;
        _chkAutoDetect.Height = 26;
        _chkAutoDetect.ForeColor = Color.FromArgb(55, 65, 81);
        _chkAutoDetect.CheckedChanged += (_, _) =>
        {
            if (_chkAutoDetect.Checked)
            {
                _autoDetectTimer.Start();
                SetRecognitionStatus("Dang bat che do nhan dien tu dong...", false);
            }
            else
            {
                _autoDetectTimer.Stop();
                SetRecognitionStatus("Da tat che do nhan dien tu dong.", false);
            }
        };
        SetRecognitionStatus("San sang nhan dien xe vao.", true);

        group.Controls.Add(cameraPanel);
        group.Controls.Add(MakeLabel("Bien so"));
        group.Controls.Add(_txtEntryPlate);
        group.Controls.Add(MakeLabel("Loai xe"));
        group.Controls.Add(_cboVehicleType);
        group.Controls.Add(MakeLabel("Thoi gian vao"));
        group.Controls.Add(_dtEntry);
        group.Controls.Add(btnDetect);
        group.Controls.Add(btnManual);
        group.Controls.Add(_chkAutoDetect);
        group.Controls.Add(_lblRecognitionStatus);
        return group;
    }

    private Control BuildPricingBox()
    {
        var group = MakeGroup("Dieu chinh gia tien", 350, 282);
        group.Controls.Add(MakeSmallHint("Don vi: VND. He thong tinh theo gio dau va moi gio tiep theo."));

        foreach (var type in Enum.GetValues<VehicleType>())
        {
            var rule = _parkingLot.PaymentService.PricingRules[type];
            group.Controls.Add(MakeLabel(type.ToString()));
            var row = new FlowLayoutPanel { Width = 300, Height = 34, FlowDirection = FlowDirection.LeftToRight };
            var first = MakeMoneyInput(rule.FirstHourFee);
            var next = MakeMoneyInput(rule.NextHourFee);
            _firstHourInputs[type] = first;
            _nextHourInputs[type] = next;
            row.Controls.Add(first);
            row.Controls.Add(next);
            group.Controls.Add(row);
        }

        var save = MakePrimaryButton("Luu bang gia");
        save.Click += SavePricingClick;
        group.Controls.Add(save);
        return group;
    }

    private Control BuildZoneConfigBox()
    {
        var group = MakeGroup("Phan vung bai xe", 350, 222);
        group.Controls.Add(MakeSmallHint("Chia suc chua rieng cho xe dap, xe may va o to."));

        foreach (var zone in _parkingLot.GetZones())
        {
            group.Controls.Add(MakeLabel(zone.Name));
            var input = MakeCapacityInput(zone.Capacity);
            _zoneCapacityInputs[zone.VehicleType] = input;
            group.Controls.Add(input);
        }

        var save = MakePrimaryButton("Luu suc chua tung khu");
        save.Click += SaveZonesClick;
        group.Controls.Add(save);
        return group;
    }

    private Control BuildExitBox()
    {
        var group = MakeGroup("Xe ra", 350, 188);
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

    private Control BuildCameraPanel()
    {
        var panel = new Panel
        {
            Width = 300,
            Height = 68,
            BackColor = Color.FromArgb(15, 23, 42),
            Margin = new Padding(0, 2, 0, 8)
        };
        var lens = new Panel
        {
            Width = 42,
            Height = 42,
            Left = 18,
            Top = 13,
            BackColor = Color.FromArgb(37, 99, 235)
        };
        var title = new Label
        {
            Text = "CAMERA AI - CONG VAO",
            Left = 74,
            Top = 12,
            Width = 210,
            Height = 22,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };
        var status = new Label
        {
            Text = "San sang quet bien so",
            Left = 74,
            Top = 35,
            Width = 210,
            Height = 22,
            ForeColor = Color.FromArgb(147, 197, 253)
        };
        panel.Controls.Add(lens);
        panel.Controls.Add(title);
        panel.Controls.Add(status);
        return panel;
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
            SetRecognitionStatus($"Da nhan xe {plate} vao bai luc {_dtEntry.Value:HH:mm:ss}.", true);
            MessageBox.Show("Da ghi nhan xe vao bai.", "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _txtEntryPlate.Clear();
            _dtEntry.Value = DateTime.Now;
            LoadDashboard();
        }
        catch (Exception ex)
        {
            SetRecognitionStatus(ex.Message, false);
            MessageBox.Show(ex.Message, "Khong the cho xe vao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void DetectAndCheckInVehicle(bool showPopup)
    {
        try
        {
            _dtEntry.Value = DateTime.Now;
            _txtEntryPlate.Text = GeneratePlate();
            _cboVehicleType.SelectedItem = GenerateVehicleType();

            var plate = RequirePlate(_txtEntryPlate.Text);
            var type = (VehicleType)_cboVehicleType.SelectedItem!;
            _parkingLot.CheckIn(plate, type, _dtEntry.Value, _currentUser.Id);
            AnimateBarrier();
            LoadDashboard();
            SetRecognitionStatus($"Nhan dien thanh cong: {plate} - {type}. Barie da mo.", true);

            if (showPopup)
            {
                MessageBox.Show(
                    $"He thong da tu dong nhan dien xe vao.\nBien so: {plate}\nLoai xe: {type}\nThoi gian: {_dtEntry.Value:dd/MM/yyyy HH:mm:ss}",
                    "Nhan dien thanh cong",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            SetRecognitionStatus(ex.Message, false);
            if (showPopup)
            {
                MessageBox.Show(ex.Message, "Nhan dien that bai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

    private void SavePricingClick(object? sender, EventArgs e)
    {
        try
        {
            foreach (var type in Enum.GetValues<VehicleType>())
            {
                _parkingLot.PaymentService.UpdateRule(
                    type,
                    _firstHourInputs[type].Value,
                    _nextHourInputs[type].Value);
            }

            MessageBox.Show("Da cap nhat bang gia tinh phi.", "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Khong the luu bang gia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SaveZonesClick(object? sender, EventArgs e)
    {
        try
        {
            foreach (var type in Enum.GetValues<VehicleType>())
            {
                _parkingLot.UpdateZoneCapacity(type, (int)_zoneCapacityInputs[type].Value);
            }

            LoadDashboard();
            MessageBox.Show("Da cap nhat suc chua tung khu.", "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Khong the luu phan vung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

    private string GeneratePlate()
    {
        var prefixes = new[] { "29A", "30F", "36B", "37A", "43C", "51G", "60A", "75B" };
        return $"{prefixes[_random.Next(prefixes.Length)]}-{_random.Next(10000, 99999)}";
    }

    private VehicleType GenerateVehicleType()
    {
        var types = new[] { VehicleType.Motorbike, VehicleType.Motorbike, VehicleType.Car, VehicleType.Bicycle };
        return types[_random.Next(types.Length)];
    }

    private void SetRecognitionStatus(string message, bool success)
    {
        _lblRecognitionStatus.Text = message;
        _lblRecognitionStatus.Width = 300;
        _lblRecognitionStatus.Height = 42;
        _lblRecognitionStatus.ForeColor = success ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
        _lblRecognitionStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
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

    private static Label MakeSmallHint(string text)
    {
        return new Label
        {
            Text = text,
            Width = 300,
            Height = 34,
            ForeColor = Color.FromArgb(107, 114, 128)
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

    private static NumericUpDown MakeMoneyInput(decimal value)
    {
        return new NumericUpDown
        {
            Width = 145,
            Height = 30,
            Minimum = 0,
            Maximum = 10000000,
            Increment = 1000,
            ThousandsSeparator = true,
            Value = value,
            Margin = new Padding(0, 0, 8, 0)
        };
    }

    private static NumericUpDown MakeCapacityInput(int value)
    {
        return new NumericUpDown
        {
            Width = 300,
            Height = 30,
            Minimum = 0,
            Maximum = 500,
            Value = value,
            Margin = new Padding(0, 0, 0, 4)
        };
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
