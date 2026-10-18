using System.ComponentModel;
using System.Globalization;

namespace CryptoTicker;

public class frmCryptoStockTicker : Form
{
    private static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");
    private static readonly Color Up = Color.Green;
    private static readonly Color Down = Color.Red;
    private const int MaxLogRows = 500;

    // ----- Control -----
    private Button btnStart, btnPause;
    private Label lblBTCPrice;
    private DataGridView dgvHistoryLog;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblState, lblSpring, lblLastUpdate, lblSyncCount;

    // ----- Dữ liệu -----
    private readonly BindingList<PriceLogModel> _log = new();

    // Trạng thái chỉ được đụng bởi UI thread
    private int _syncCount;
    private bool _hasPrev;
    private decimal _prevPrice;

    // Trạng thái dùng bởi luồng nền (bảo vệ bằng lock)
    private readonly object _priceLock = new();
    private decimal _currentPrice = 65000m;

    private CancellationTokenSource _cts;   // != null nghĩa là đang giám sát

    public frmCryptoStockTicker()
    {
        BuildUi();
        SetupGrid();
        dgvHistoryLog.DataSource = _log;
        SetRunning(false);
    }

    // ===================== DỰNG GIAO DIỆN =====================
    private void BuildUi()
    {
        Text = "Giám sát tỷ giá BTC/USD thời gian thực";
        ClientSize = new Size(820, 560);
        MinimumSize = new Size(700, 420);
        StartPosition = FormStartPosition.CenterScreen;

        // ----- Khu vực điều khiển -----
        var pnlControl = new Panel { Dock = DockStyle.Top, Height = 56 };
        btnStart = new Button { Text = "Bắt đầu Giám sát", Left = 15, Top = 12, Width = 170, Height = 32 };
        btnPause = new Button { Text = "Tạm dừng", Left = 195, Top = 12, Width = 130, Height = 32 };
        btnStart.Click += (s, e) => StartMonitoring();
        btnPause.Click += (s, e) => PauseMonitoring();
        pnlControl.Controls.AddRange(new Control[] { btnStart, btnPause });

        // ----- Giá BTC -----
        var pnlPrice = new Panel { Dock = DockStyle.Top, Height = 110, BorderStyle = BorderStyle.FixedSingle };
        lblBTCPrice = new Label
        {
            Dock = DockStyle.Fill, Text = "BTC/USD: --",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 34f, FontStyle.Bold)
        };
        pnlPrice.Controls.Add(lblBTCPrice);

        // ----- StatusStrip -----
        statusStrip = new StatusStrip();
        lblState = new ToolStripStatusLabel("Đã dừng");
        lblSpring = new ToolStripStatusLabel { Spring = true };
        lblLastUpdate = new ToolStripStatusLabel("Cập nhật lần cuối: --:--:--");
        lblSyncCount = new ToolStripStatusLabel("Số lượt đồng bộ: 0") { BorderSides = ToolStripStatusLabelBorderSides.Left };
        statusStrip.Items.AddRange(new ToolStripItem[] { lblState, lblSpring, lblLastUpdate, lblSyncCount });

        // ----- Lưới nhật ký -----
        dgvHistoryLog = new DataGridView { Dock = DockStyle.Fill };
        dgvHistoryLog.CellFormatting += dgvHistoryLog_CellFormatting;

        // Fill trước, Bottom, rồi các Top (Add sau nằm ngoài cùng)
        Controls.Add(dgvHistoryLog);
        Controls.Add(statusStrip);
        Controls.Add(pnlPrice);
        Controls.Add(pnlControl);

        FormClosing += (s, e) => StopLoop();    // đóng form thì dừng luồng nền
    }

    private void SetupGrid()
    {
        var g = dgvHistoryLog;
        g.AutoGenerateColumns = false;
        g.Columns.Clear();
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.RowHeadersVisible = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // FormatProvider cố định en-US để số hiển thị đồng nhất (65,123.45)
        var price = new DataGridViewCellStyle
        {
            Format = "N2", FormatProvider = Us, Alignment = DataGridViewContentAlignment.MiddleRight
        };
        var change = new DataGridViewCellStyle
        {
            Format = "+#,##0.00;-#,##0.00;0.00", FormatProvider = Us, Alignment = DataGridViewContentAlignment.MiddleRight
        };
        var pct = new DataGridViewCellStyle
        {
            Format = "+0.000'%';-0.000'%';0.000'%'", FormatProvider = Us, Alignment = DataGridViewContentAlignment.MiddleRight
        };

        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIndex", HeaderText = "Lượt", DataPropertyName = nameof(PriceLogModel.Index), FillWeight = 40 });
        g.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colTime", HeaderText = "Thời gian", DataPropertyName = nameof(PriceLogModel.Time), FillWeight = 80,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "HH:mm:ss" }
        });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Giá (USD)", DataPropertyName = nameof(PriceLogModel.Price), FillWeight = 100, DefaultCellStyle = price });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colChange", HeaderText = "Thay đổi", DataPropertyName = nameof(PriceLogModel.Change), FillWeight = 80, DefaultCellStyle = change });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPercent", HeaderText = "% thay đổi", DataPropertyName = nameof(PriceLogModel.ChangePercent), FillWeight = 80, DefaultCellStyle = pct });
        g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTrend", HeaderText = "Xu hướng", DataPropertyName = nameof(PriceLogModel.Trend), FillWeight = 90 });
    }

    // Tô màu xanh/đỏ cho các cột Thay đổi, %, Xu hướng
    private void dgvHistoryLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        string col = dgvHistoryLog.Columns[e.ColumnIndex].Name;
        if (col != "colChange" && col != "colPercent" && col != "colTrend") return;

        if (dgvHistoryLog.Rows[e.RowIndex].DataBoundItem is not PriceLogModel m) return;
        e.CellStyle.ForeColor = m.Change > 0 ? Up : m.Change < 0 ? Down : dgvHistoryLog.DefaultCellStyle.ForeColor;
    }

    // ===================== ĐIỀU KHIỂN GIÁM SÁT =====================
    private void SetRunning(bool running)
    {
        btnStart.Enabled = !running;
        btnPause.Enabled = running;
        lblState.Text = running ? "Đang giám sát..." : "Đã tạm dừng";
    }

    private void StartMonitoring()
    {
        if (_cts != null) return;              // đang chạy rồi

        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;
        SetRunning(true);

        // Vòng lặp vô hạn chạy ở Background Thread (thread pool)
        _ = Task.Run(() => MonitorLoopAsync(token), token);
    }

    private void PauseMonitoring()
    {
        StopLoop();
        SetRunning(false);
    }

    private void StopLoop()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    // ===================== LUỒNG NỀN =====================
    /// <summary>
    /// Chạy ở Background Thread: cứ 2 giây sinh một tỷ giá mới.
    /// KHÔNG được đụng vào control ở đây - chỉ gọi UpdateUi() để chuyển dữ liệu về UI thread.
    /// </summary>
    private async Task MonitorLoopAsync(CancellationToken token)
    {
        var rnd = new Random();
        try
        {
            while (!token.IsCancellationRequested)
            {
                decimal newPrice;
                lock (_priceLock)
                {
                    _currentPrice = NextPrice(_currentPrice, rnd);
                    newPrice = _currentPrice;
                }

                UpdateUi(newPrice, DateTime.Now, token);   // an toàn luồng (xem bên dưới)

                await Task.Delay(2000, token);             // chu kỳ 2 giây, hủy được ngay
            }
        }
        catch (OperationCanceledException)
        {
            // Bấm Tạm dừng / đóng form: thoát vòng lặp bình thường
        }
    }

    /// <summary>Random walk ±0.6% mỗi lần, giới hạn trong khoảng 20.000 - 120.000.</summary>
    private static decimal NextPrice(decimal current, Random rnd)
    {
        double factor = 1 + (rnd.NextDouble() - 0.5) * 0.012;
        decimal next = Math.Round(current * (decimal)factor, 2);
        return Math.Clamp(next, 20_000m, 120_000m);
    }

    // ===================== CẬP NHẬT UI AN TOÀN LUỒNG =====================
    /// <summary>
    /// Có thể gọi từ BẤT KỲ luồng nào. Nếu đang ở luồng phụ thì đẩy chính hàm này
    /// về Main UI Thread bằng Invoke; phần cập nhật control chỉ chạy ở UI thread.
    /// </summary>
    private void UpdateUi(decimal price, DateTime time, CancellationToken token)
    {
        if (IsDisposed || !IsHandleCreated) return;

        if (InvokeRequired)
        {
            try
            {
                Invoke(new Action(() => UpdateUi(price, time, token)));
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) when (IsDisposed || Disposing || !IsHandleCreated)
            {
                // Form đang đóng đúng lúc luồng nền gọi Invoke: bỏ qua
            }
            return;
        }

        // ---------- Từ đây trở xuống: chắc chắn đang ở UI thread ----------
        if (token.IsCancellationRequested) return;   // bỏ cập nhật "đến muộn" sau khi Tạm dừng

        decimal change = _hasPrev ? price - _prevPrice : 0m;
        double pct = _hasPrev && _prevPrice != 0 ? (double)(change / _prevPrice * 100m) : 0d;

        Color color = change > 0 ? Up : change < 0 ? Down : SystemColors.ControlText;
        string arrow = change > 0 ? "▲" : change < 0 ? "▼" : "■";
        string trend = !_hasPrev ? "Khởi tạo" : change > 0 ? "▲ Tăng" : change < 0 ? "▼ Giảm" : "■ Không đổi";

        lblBTCPrice.Text = $"{arrow} ${price.ToString("N2", Us)}";
        lblBTCPrice.ForeColor = color;

        _syncCount++;
        _log.Insert(0, new PriceLogModel
        {
            Index = _syncCount,
            Time = time,
            Price = price,
            Change = change,
            ChangePercent = pct,
            Trend = trend
        });
        while (_log.Count > MaxLogRows) _log.RemoveAt(_log.Count - 1);

        lblLastUpdate.Text = $"Cập nhật lần cuối: {time:HH:mm:ss}";
        lblSyncCount.Text = $"Số lượt đồng bộ: {_syncCount}";

        _prevPrice = price;
        _hasPrev = true;
    }
}
