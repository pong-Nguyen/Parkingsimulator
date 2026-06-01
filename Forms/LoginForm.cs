using SmartParkingSystem.Models;
using SmartParkingSystem.Services;

namespace SmartParkingSystem.Forms;

public class LoginForm : Form
{
    private readonly AuthService _authService;
    private readonly ParkingLot _parkingLot;
    private readonly StatisticsService _statisticsService;
    private readonly TextBox _txtUsername = new();
    private readonly TextBox _txtPassword = new();
    private readonly Label _lblError = new();

    public LoginForm(AuthService authService, ParkingLot parkingLot, StatisticsService statisticsService)
    {
        _authService = authService;
        _parkingLot = parkingLot;
        _statisticsService = statisticsService;
        BuildUi();
    }

    private void BuildUi()
    {
        Text = "Dang nhap - Smart Parking";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(420, 480);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 10F);

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(36)
        };
        Controls.Add(panel);

        var title = new Label
        {
            Text = "SMART PARKING",
            AutoSize = false,
            Height = 46,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55)
        };
        panel.Controls.Add(title);

        var subtitle = new Label
        {
            Text = "He thong quan ly bai gui xe thong minh",
            AutoSize = false,
            Height = 42,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(75, 85, 99)
        };
        panel.Controls.Add(subtitle);
        subtitle.BringToFront();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 240,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(0, 18, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.Controls.Add(layout);
        layout.BringToFront();

        layout.Controls.Add(MakeLabel("Tai khoan"), 0, 0);
        ConfigureTextBox(_txtUsername, "admin");
        layout.Controls.Add(_txtUsername, 0, 1);

        layout.Controls.Add(MakeLabel("Mat khau"), 0, 3);
        ConfigureTextBox(_txtPassword, "admin123");
        _txtPassword.UseSystemPasswordChar = true;
        layout.Controls.Add(_txtPassword, 0, 4);

        var btnLogin = new Button
        {
            Text = "Dang nhap",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += LoginClick;
        layout.Controls.Add(btnLogin, 0, 5);

        _lblError.Dock = DockStyle.Fill;
        _lblError.TextAlign = ContentAlignment.MiddleCenter;
        _lblError.ForeColor = Color.FromArgb(220, 38, 38);
        layout.Controls.Add(_lblError, 0, 6);

        var hint = new Label
        {
            Text = "Mac dinh: admin/admin123 hoac staff/staff123",
            Dock = DockStyle.Bottom,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(107, 114, 128)
        };
        panel.Controls.Add(hint);

        AcceptButton = btnLogin;
    }

    private void LoginClick(object? sender, EventArgs e)
    {
        var user = _authService.Login(_txtUsername.Text, _txtPassword.Text);
        if (user is null)
        {
            _lblError.Text = "Sai tai khoan hoac mat khau.";
            return;
        }

        Hide();
        using var main = new MainForm(user, _parkingLot, _statisticsService);
        main.ShowDialog(this);
        Show();
        _txtPassword.Clear();
    }

    private static Label MakeLabel(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(55, 65, 81),
            TextAlign = ContentAlignment.BottomLeft
        };
    }

    private static void ConfigureTextBox(TextBox textBox, string text)
    {
        textBox.Text = text;
        textBox.Dock = DockStyle.Fill;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = new Font("Segoe UI", 11F);
    }
}
