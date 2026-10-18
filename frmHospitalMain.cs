namespace HospitalManager;

/// <summary>Form cha MDI.</summary>
public class frmHospitalMain : Form
{
    private const string DefaultDuty = "BS. Nguyễn Văn Hùng";

    private MenuStrip menuStrip;
    private ToolStripMenuItem mnuSystem, mnuLogin, mnuLogout, mnuReload, mnuExit;
    private ToolStripMenuItem mnuManage, mnuPatients, mnuDoctors, mnuDepartments;
    private ToolStripMenuItem mnuWindow, mnuCascade, mnuTileH, mnuTileV, mnuCloseAll;

    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblDuty, lblSpring, lblStatus;
    private ToolStripProgressBar progressBar;

    private readonly System.Windows.Forms.Timer _tmrReload = new() { Interval = 40 };

    public frmHospitalMain()
    {
        BuildUi();
        SetLoggedIn(DefaultDuty);
    }

    // ===================== DỰNG GIAO DIỆN =====================
    private void BuildUi()
    {
        Text = "Hệ thống quản lý bệnh viện";
        IsMdiContainer = true;                       // bắt buộc: form cha MDI
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(900, 600);

        // ---- MenuStrip ----
        menuStrip = new MenuStrip();

        mnuSystem = new ToolStripMenuItem("&Hệ thống");
        mnuLogin = new ToolStripMenuItem("Đăng nhập");
        mnuLogout = new ToolStripMenuItem("Đăng xuất");
        mnuReload = new ToolStripMenuItem("Nạp lại dữ liệu") { ShortcutKeys = Keys.F5 };
        mnuExit = new ToolStripMenuItem("Thoát");
        mnuSystem.DropDownItems.AddRange(new ToolStripItem[] {
            mnuLogin, mnuLogout, new ToolStripSeparator(), mnuReload, new ToolStripSeparator(), mnuExit });

        mnuManage = new ToolStripMenuItem("&Quản lý");
        mnuPatients = new ToolStripMenuItem("Hồ sơ Bệnh nhân");
        mnuDoctors = new ToolStripMenuItem("Danh mục Bác sĩ");
        mnuDepartments = new ToolStripMenuItem("Danh mục Khoa/Phòng");
        mnuManage.DropDownItems.AddRange(new ToolStripItem[] { mnuPatients, mnuDoctors, mnuDepartments });

        mnuWindow = new ToolStripMenuItem("&Cửa sổ");
        mnuCascade = new ToolStripMenuItem("Sắp xếp xếp tầng (Cascade)");
        mnuTileH = new ToolStripMenuItem("Sắp xếp ngang (Tile Horizontal)");
        mnuTileV = new ToolStripMenuItem("Sắp xếp dọc (Tile Vertical)");
        mnuCloseAll = new ToolStripMenuItem("Đóng tất cả");
        mnuWindow.DropDownItems.AddRange(new ToolStripItem[] {
            mnuCascade, mnuTileH, mnuTileV, new ToolStripSeparator(), mnuCloseAll });

        menuStrip.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuManage, mnuWindow });
        MainMenuStrip = menuStrip;
        // Menu "Cửa sổ" tự liệt kê các form con đang mở ở cuối danh sách
        menuStrip.MdiWindowListItem = mnuWindow;

        // ---- StatusStrip ----
        statusStrip = new StatusStrip();
        lblDuty = new ToolStripStatusLabel();
        lblSpring = new ToolStripStatusLabel { Spring = true };
        lblStatus = new ToolStripStatusLabel("Sẵn sàng");
        progressBar = new ToolStripProgressBar { Minimum = 0, Maximum = 100, Width = 160 };
        statusStrip.Items.AddRange(new ToolStripItem[] { lblDuty, lblSpring, lblStatus, progressBar });

        Controls.Add(statusStrip);
        Controls.Add(menuStrip);

        // ---- Sự kiện ----
        mnuLogin.Click += mnuLogin_Click;
        mnuLogout.Click += mnuLogout_Click;
        mnuReload.Click += mnuReload_Click;
        mnuExit.Click += (s, e) => Close();

        mnuPatients.Click += (s, e) => OpenSingleChild<frmPatientProfiles>();
        mnuDoctors.Click += (s, e) => OpenSingleChild<frmDoctorSchedule>();
        mnuDepartments.Click += (s, e) => OpenSingleChild<frmDepartments>();

        mnuCascade.Click += (s, e) => ArrangeWindows(MdiLayout.Cascade);
        mnuTileH.Click += (s, e) => ArrangeWindows(MdiLayout.TileHorizontal);
        mnuTileV.Click += (s, e) => ArrangeWindows(MdiLayout.TileVertical);
        mnuCloseAll.Click += (s, e) => CloseAllChildren();

        _tmrReload.Tick += tmrReload_Tick;
        FormClosing += frmHospitalMain_FormClosing;
        FormClosed += (s, e) => _tmrReload.Dispose();
    }

    // ===================== QUẢN LÝ FORM CON (CHỐNG MỞ TRÙNG) =====================
    /// <summary>
    /// Duyệt MdiChildren: nếu form kiểu T đã mở thì Activate(), chưa có thì tạo mới,
    /// gán MdiParent = this rồi Show().
    /// </summary>
    private void OpenSingleChild<T>() where T : Form, new()
    {
        foreach (Form child in MdiChildren)
        {
            if (child.GetType() == typeof(T))
            {
                if (child.WindowState == FormWindowState.Minimized)
                    child.WindowState = FormWindowState.Normal;
                child.Activate();
                return;
            }
        }

        var frm = new T { MdiParent = this };
        frm.Show();
    }

    // ===================== SẮP XẾP CỬA SỔ =====================
    private void ArrangeWindows(MdiLayout layout)
    {
        // Form đang Maximized/Minimized sẽ không thấy hiệu ứng sắp xếp => đưa về Normal trước
        foreach (Form child in MdiChildren)
            child.WindowState = FormWindowState.Normal;

        LayoutMdi(layout);
    }

    private void CloseAllChildren()
    {
        foreach (Form child in MdiChildren)
            child.Close();
    }

    // ===================== HỆ THỐNG =====================
    private void SetLoggedIn(string doctorName)
    {
        bool loggedIn = !string.IsNullOrWhiteSpace(doctorName);
        lblDuty.Text = loggedIn ? $"Bác sĩ trực: {doctorName}" : "Chưa đăng nhập";

        mnuLogin.Enabled = !loggedIn;
        mnuLogout.Enabled = loggedIn;
        mnuReload.Enabled = loggedIn;
        mnuManage.Enabled = loggedIn;
        mnuWindow.Enabled = loggedIn;
    }

    private void mnuLogin_Click(object sender, EventArgs e)
    {
        string name = PromptText("Đăng nhập", "Nhập tên Bác sĩ/Sĩ quan trực:", DefaultDuty);
        if (!string.IsNullOrWhiteSpace(name))
            SetLoggedIn(name);
    }

    private void mnuLogout_Click(object sender, EventArgs e)
    {
        var r = MessageBox.Show("Bạn có chắc muốn đăng xuất? Tất cả cửa sổ đang mở sẽ bị đóng.",
            "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        CloseAllChildren();
        SetLoggedIn(null);
        lblStatus.Text = "Đã đăng xuất";
    }

    // Mô phỏng nạp dữ liệu: thanh tiến trình chạy 0 -> 100%, xong mới nạp và báo cho form con
    private void mnuReload_Click(object sender, EventArgs e)
    {
        progressBar.Value = 0;
        lblStatus.Text = "Đang nạp lại dữ liệu...";
        mnuReload.Enabled = false;
        _tmrReload.Start();
    }

    private void tmrReload_Tick(object sender, EventArgs e)
    {
        progressBar.Value = Math.Min(progressBar.Value + 5, progressBar.Maximum);
        if (progressBar.Value < progressBar.Maximum) return;

        _tmrReload.Stop();
        DataStore.Load();
        foreach (Form child in MdiChildren)
            if (child is IReloadable r) r.ReloadData();

        lblStatus.Text = $"Đã nạp lại dữ liệu lúc {DateTime.Now:HH:mm:ss}";
        progressBar.Value = 0;
        mnuReload.Enabled = true;
    }

    private void frmHospitalMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason != CloseReason.UserClosing) return;

        var r = MessageBox.Show("Bạn có chắc muốn thoát chương trình?", "Thoát",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) e.Cancel = true;
    }

    private string PromptText(string title, string label, string defaultValue)
    {
        using var f = new Form
        {
            Text = title, FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(360, 112), MaximizeBox = false, MinimizeBox = false
        };
        var lbl = new Label { Text = label, Left = 12, Top = 12, AutoSize = true };
        var txt = new TextBox { Left = 12, Top = 36, Width = 336, Text = defaultValue };
        var ok = new Button { Text = "OK", Left = 192, Top = 72, Width = 75, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Hủy", Left = 273, Top = 72, Width = 75, DialogResult = DialogResult.Cancel };
        f.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
        f.AcceptButton = ok;
        f.CancelButton = cancel;
        return f.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : null;
    }
}
