namespace RevenueAnalytics;

public class frmRevenueAnalytics : Form
{
    private static readonly string[] Branches = { "Hà Nội", "Đà Nẵng", "TP. Hồ Chí Minh", "Cần Thơ" };

    // ----- Control -----
    private DateTimePicker dtpFrom, dtpTo;
    private ComboBox cboBranch;
    private Button btnAnalyze, btnCancel;
    private TextBox txtNote;
    private ProgressBar pgbLoading;
    private Label lblProcessingStatus, lblSummary;
    private DataGridView dgvRevenueData;

    // Nguồn hủy tác vụ: tạo mới cho mỗi lần thống kê
    private CancellationTokenSource _cts;

    public frmRevenueAnalytics()
    {
        BuildUi();
        SetupGrid();
    }

    // ===================== DỰNG GIAO DIỆN =====================
    private void BuildUi()
    {
        Text = "Bảng điều khiển thống kê doanh thu";
        ClientSize = new Size(960, 640);
        MinimumSize = new Size(820, 500);
        StartPosition = FormStartPosition.CenterScreen;

        // ----- Khu vực tiêu chí -----
        var grpCriteria = new GroupBox { Text = "Tiêu chí thống kê", Dock = DockStyle.Top, Height = 112 };

        dtpFrom = new DateTimePicker
        {
            Left = 90, Top = 28, Width = 140,
            Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy",
            Value = DateTime.Today.AddMonths(-1)
        };
        dtpTo = new DateTimePicker
        {
            Left = 320, Top = 28, Width = 140,
            Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy",
            Value = DateTime.Today
        };
        cboBranch = new ComboBox { Left = 560, Top = 28, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cboBranch.Items.Add("Tất cả chi nhánh");
        cboBranch.Items.AddRange(Branches);
        cboBranch.SelectedIndex = 0;

        btnAnalyze = new Button { Text = "Thống Kê Doanh Thu", Left = 15, Top = 66, Width = 170, Height = 32 };
        btnCancel = new Button { Text = "Hủy Thao Tác", Left = 195, Top = 66, Width = 140, Height = 32, Enabled = false };
        btnAnalyze.Click += btnAnalyze_Click;
        btnCancel.Click += btnCancel_Click;

        // Ô nhập để chứng minh UI vẫn gõ được khi đang xử lý nền
        txtNote = new TextBox { Left = 480, Top = 70, Width = 280, PlaceholderText = "Thử gõ chữ vào đây khi đang thống kê..." };

        grpCriteria.Controls.AddRange(new Control[] {
            L("Từ ngày:", 15, 28, 70), dtpFrom,
            L("Đến ngày:", 245, 28, 70), dtpTo,
            L("Chi nhánh:", 480, 28, 75), cboBranch,
            btnAnalyze, btnCancel,
            L("Ghi chú:", 400, 70, 75), txtNote });

        // ----- Khu vực trạng thái -----
        var pnlStatus = new Panel { Dock = DockStyle.Top, Height = 62 };
        pgbLoading = new ProgressBar
        {
            Left = 12, Top = 8, Height = 22, Width = 936,
            Minimum = 0, Maximum = 100, Style = ProgressBarStyle.Continuous,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        lblProcessingStatus = new Label
        {
            Left = 12, Top = 36, Width = 936, Height = 22,
            Text = "Sẵn sàng.", AutoSize = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        pnlStatus.Controls.AddRange(new Control[] { pgbLoading, lblProcessingStatus });

        // ----- Dòng tổng kết ở dưới cùng -----
        lblSummary = new Label
        {
            Dock = DockStyle.Bottom, Height = 30, Text = "",
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0),
            Font = new Font(Font, FontStyle.Bold)
        };

        // ----- Lưới kết quả -----
        dgvRevenueData = new DataGridView { Dock = DockStyle.Fill };

        // Thứ tự Add: Fill trước, rồi Bottom, rồi các Top (cái nào Add sau sẽ nằm ngoài cùng)
        Controls.Add(dgvRevenueData);
        Controls.Add(lblSummary);
        Controls.Add(pnlStatus);
        Controls.Add(grpCriteria);

        FormClosing += (s, e) => _cts?.Cancel();   // đóng form thì dừng tiến trình ngầm
    }

    private static Label L(string text, int x, int y, int w) =>
        new Label { Text = text, Left = x, Top = y + 4, Width = w, AutoSize = false };

    private void SetupGrid()
    {
        var g = dgvRevenueData;
        g.AutoGenerateColumns = false;
        g.Columns.Clear();
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.RowHeadersVisible = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        var money = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight };

        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "Mã GD", DataPropertyName = nameof(TransactionModel.Id), FillWeight = 70 });
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDate", HeaderText = "Thời gian", DataPropertyName = nameof(TransactionModel.Date), FillWeight = 110,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
        });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBranch", HeaderText = "Chi nhánh", DataPropertyName = nameof(TransactionModel.Branch), FillWeight = 110 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItem", HeaderText = "Sản phẩm", DataPropertyName = nameof(TransactionModel.Item), FillWeight = 120 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty", HeaderText = "SL", DataPropertyName = nameof(TransactionModel.Quantity), FillWeight = 40 });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Đơn giá", DataPropertyName = nameof(TransactionModel.UnitPrice), FillWeight = 80, DefaultCellStyle = money });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAmount", HeaderText = "Thành tiền", DataPropertyName = nameof(TransactionModel.Amount), FillWeight = 90, DefaultCellStyle = money });
    }

    // ===================== TRẠNG THÁI GIAO DIỆN =====================
    private void SetBusy(bool busy)
    {
        btnAnalyze.Enabled = !busy;
        btnCancel.Enabled = busy;
        dtpFrom.Enabled = dtpTo.Enabled = cboBranch.Enabled = !busy;
    }

    // ===================== THỐNG KÊ (ASYNC/AWAIT) =====================
    private async void btnAnalyze_Click(object sender, EventArgs e)
    {
        DateTime from = dtpFrom.Value.Date;
        DateTime to = dtpTo.Value.Date;

        if (from > to)
        {
            MessageBox.Show("\"Từ ngày\" phải nhỏ hơn hoặc bằng \"Đến ngày\".", "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if ((to - from).TotalDays > 366)
        {
            MessageBox.Show("Vui lòng chọn khoảng thời gian tối đa 1 năm.", "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Lấy giá trị từ control TRƯỚC khi vào luồng nền (luồng nền không được đụng vào control)
        string branch = cboBranch.SelectedIndex == 0 ? null : cboBranch.Text;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // Progress<T> được tạo trên UI thread => callback tự động chạy lại trên UI thread
        var progress = new Progress<(int Percent, string Message)>(p =>
        {
            // Bỏ qua báo cáo "đến muộn" sau khi đã bấm Hủy để không ghi đè thông báo
            if (_cts == null || _cts.IsCancellationRequested) return;
            pgbLoading.Value = p.Percent;
            lblProcessingStatus.Text = $"{p.Message} ({p.Percent}%)";
        });

        SetBusy(true);
        dgvRevenueData.DataSource = null;
        lblSummary.Text = "";
        pgbLoading.Value = 0;
        lblProcessingStatus.Text = "Bắt đầu xử lý...";

        try
        {
            // Đẩy việc nặng xuống Background Thread, UI thread được giải phóng trong lúc await
            List<TransactionModel> result =
                await Task.Run(() => QueryRevenue(from, to, branch, progress, token), token);

            // Quay lại UI thread ở đây
            dgvRevenueData.DataSource = result;
            long total = result.Sum(t => t.Amount);
            lblSummary.Text = $"Tổng: {result.Count:N0} giao dịch  |  Doanh thu: {total:N0} VNĐ";
            pgbLoading.Value = 100;
            lblProcessingStatus.Text = "Hoàn tất thống kê!";
        }
        catch (OperationCanceledException)
        {
            pgbLoading.Value = 0;
            lblProcessingStatus.Text = "Đã hủy thao tác!";
            MessageBox.Show("Đã hủy thao tác!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            pgbLoading.Value = 0;
            lblProcessingStatus.Text = "Có lỗi xảy ra.";
            MessageBox.Show("Lỗi khi thống kê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetBusy(false);
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        if (_cts == null || _cts.IsCancellationRequested) return;

        _cts.Cancel();                 // phát tín hiệu hủy cho luồng nền
        btnCancel.Enabled = false;     // tránh bấm nhiều lần
        lblProcessingStatus.Text = "Đang hủy...";
    }

    // ===================== CÔNG VIỆC NẶNG (CHẠY Ở BACKGROUND THREAD) =====================
    /// <summary>
    /// Giả lập truy xuất CSDL + tổng hợp giao dịch trong 4-6 giây.
    /// Hàm này chạy trên luồng nền nên TUYỆT ĐỐI không truy cập control của form.
    /// </summary>
    private static List<TransactionModel> QueryRevenue(
        DateTime from, DateTime to, string branch,
        IProgress<(int Percent, string Message)> progress, CancellationToken token)
    {
        var rnd = new Random();
        const int steps = 50;
        int totalMs = rnd.Next(4000, 6001);        // 4 - 6 giây
        int delayPerStep = totalMs / steps;

        string[] items = { "Áo thun", "Giày thể thao", "Balo", "Đồng hồ", "Kính mát", "Phụ kiện" };
        long[] prices = { 150_000, 850_000, 320_000, 1_200_000, 450_000, 90_000 };

        string[] branches = branch == null ? Branches : new[] { branch };
        int totalDays = (to - from).Days + 1;
        var list = new List<TransactionModel>();
        int counter = 0;

        for (int step = 1; step <= steps; step++)
        {
            token.ThrowIfCancellationRequested();   // điểm kiểm tra hủy an toàn

            // Phần dữ liệu (các ngày) xử lý trong bước này
            int dayStart = (step - 1) * totalDays / steps;
            int dayEnd = step * totalDays / steps;

            for (int d = dayStart; d < dayEnd; d++)
            {
                DateTime day = from.AddDays(d);
                foreach (string br in branches)
                {
                    int count = rnd.Next(5, 11);
                    for (int i = 0; i < count; i++)
                    {
                        int k = rnd.Next(items.Length);
                        list.Add(new TransactionModel
                        {
                            Id = $"GD{++counter:D6}",
                            Date = day.AddMinutes(rnd.Next(8 * 60, 22 * 60)),
                            Branch = br,
                            Item = items[k],
                            Quantity = rnd.Next(1, 6),
                            UnitPrice = prices[k]
                        });
                    }
                }
            }

            Thread.Sleep(delayPerStep);             // giả lập độ trễ truy vấn CSDL

            int percent = step * 100 / steps;
            string msg = percent < 10 ? "Đang kết nối cơ sở dữ liệu..."
                       : percent < 60 ? $"Đang truy xuất giao dịch ({list.Count:N0} bản ghi)..."
                       : percent < 95 ? "Đang tổng hợp dữ liệu..."
                       : "Đang hoàn tất...";
            progress.Report((percent, msg));
        }

        token.ThrowIfCancellationRequested();
        return list.OrderBy(t => t.Date).ToList();
    }
}
