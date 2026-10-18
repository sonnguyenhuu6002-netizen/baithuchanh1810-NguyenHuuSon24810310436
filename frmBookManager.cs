using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BookManager;

public partial class frmBookManager : Form
{
    private const string CsvHeader = "MaSach,TenSach,TacGia,Gia,SoLuong,AnhBia";

    private readonly BindingList<BookModel> _books = new();
    private string _coverPath = "";      // đường dẫn ảnh bìa đang hiển thị trên form
    private string _lastKeyword = "";    // từ khóa tìm kiếm gần nhất

    public frmBookManager()
    {
        InitializeComponent();
    }

    // ===================== KHỞI TẠO =====================
    private void frmBookManager_Load(object sender, EventArgs e)
    {
        SetupGrid();

        // Mọi biến động của danh sách (Thêm/Sửa/Xóa/Import) đều đi qua ListChanged
        _books.ListChanged += (s, ev) => UpdateStatus();

        _books.Add(new BookModel { Id = "S001", Title = "Lập trình C# căn bản", Author = "Nguyễn Văn A", Price = 120000, Quantity = 15 });
        _books.Add(new BookModel { Id = "S002", Title = "Cấu trúc dữ liệu và giải thuật", Author = "Trần Thị B", Price = 185000, Quantity = 8 });
        _books.Add(new BookModel { Id = "S003", Title = "Cơ sở dữ liệu", Author = "Lê Văn C", Price = 150000, Quantity = 12 });

        dgvBooks.DataSource = _books;
        dgvBooks.SelectionChanged += dgvBooks_SelectionChanged;

        UpdateClock();
        tmrClock.Start();
        ClearForm();
    }

    private void SetupGrid()
    {
        dgvBooks.AutoGenerateColumns = false;
        dgvBooks.Columns.Clear();
        dgvBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvBooks.MultiSelect = false;
        dgvBooks.ReadOnly = true;
        dgvBooks.AllowUserToAddRows = false;
        dgvBooks.AllowUserToDeleteRows = false;
        dgvBooks.RowHeadersVisible = false;
        dgvBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        var money = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight };

        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "Mã sách", DataPropertyName = nameof(BookModel.Id), FillWeight = 60 });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Tên sách", DataPropertyName = nameof(BookModel.Title), FillWeight = 180 });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAuthor", HeaderText = "Tác giả", DataPropertyName = nameof(BookModel.Author), FillWeight = 110 });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Giá", DataPropertyName = nameof(BookModel.Price), FillWeight = 80, DefaultCellStyle = money });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty", HeaderText = "SL", DataPropertyName = nameof(BookModel.Quantity), FillWeight = 45 });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotal", HeaderText = "Thành tiền", DataPropertyName = nameof(BookModel.Total), FillWeight = 90, DefaultCellStyle = money });
        dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCover", HeaderText = "Ảnh bìa", DataPropertyName = nameof(BookModel.CoverFileName), FillWeight = 100 });
    }

    // ===================== STATUSSTRIP =====================
    private void UpdateStatus()
    {
        long total = _books.Sum(b => b.Total);
        lblCount.Text = $"Tổng số đầu sách: {_books.Count}";
        lblTotal.Text = $"Tổng giá trị kho: {total:N0} VNĐ";
    }

    private void UpdateClock() => lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

    // ===================== FORM <-> MODEL =====================
    private BookModel CurrentBook =>
        dgvBooks.SelectedRows.Count > 0 ? dgvBooks.SelectedRows[0].DataBoundItem as BookModel : null;

    private void dgvBooks_SelectionChanged(object sender, EventArgs e)
    {
        var b = CurrentBook;
        if (b == null) return;

        txtBookId.Text = b.Id;
        txtTitle.Text = b.Title;
        txtAuthor.Text = b.Author;
        txtPrice.Text = b.Price.ToString(CultureInfo.InvariantCulture);
        nudQuantity.Value = Math.Clamp(b.Quantity, (int)nudQuantity.Minimum, (int)nudQuantity.Maximum);
        _coverPath = b.CoverPath;
        txtCoverFile.Text = b.CoverFileName;
        ShowCover(_coverPath);
    }

    private void ClearForm()
    {
        dgvBooks.ClearSelection();
        txtBookId.Text = NextId();
        txtTitle.Clear();
        txtAuthor.Clear();
        txtPrice.Clear();
        nudQuantity.Value = 0;
        _coverPath = "";
        txtCoverFile.Clear();
        ShowCover("");
        txtTitle.Focus();
    }

    private string NextId()
    {
        int max = _books
            .Select(b => int.TryParse(b.Id?.AsSpan(1), out int n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"S{max + 1:D3}";
    }

    private bool ValidateInput(out long price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            Warn("Tên sách không được để trống.", txtTitle);
            return false;
        }
        if (!Regex.IsMatch(txtPrice.Text.Trim(), @"^\d+$") ||
            !long.TryParse(txtPrice.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out price) || price <= 0)
        {
            Warn("Giá phải là số nguyên dương.", txtPrice);
            return false;
        }
        return true;
    }

    private static void Warn(string msg, Control focus)
    {
        MessageBox.Show(msg, "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        focus.Focus();
    }

    // ===================== ẢNH BÌA (OpenFileDialog) =====================
    private void btnLoadCover_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Chọn ảnh bìa sách",
            Filter = "Ảnh (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            CheckFileExists = true
        };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        if (!ShowCover(ofd.FileName))
        {
            MessageBox.Show("Không thể đọc file ảnh đã chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _coverPath = ofd.FileName;
        txtCoverFile.Text = Path.GetFileName(_coverPath);

        // Nếu đang chọn một cuốn sách thì lưu luôn đường dẫn vào đối tượng đó
        var b = CurrentBook;
        if (b != null) b.CoverPath = _coverPath;
    }

    /// <summary>Nạp ảnh lên PictureBox (không khóa file). Trả về false nếu lỗi.</summary>
    private bool ShowCover(string path)
    {
        var old = picBookCover.Image;
        picBookCover.Image = null;
        old?.Dispose();

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return string.IsNullOrWhiteSpace(path);

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var img = Image.FromStream(fs);
            picBookCover.Image = new Bitmap(img);   // bản sao, nên không giữ file bị khóa
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ===================== THÊM / SỬA / XÓA =====================
    private void AddBook()
    {
        if (!ValidateInput(out long price)) return;

        _books.Add(new BookModel
        {
            Id = NextId(),
            Title = txtTitle.Text.Trim(),
            Author = txtAuthor.Text.Trim(),
            Price = price,
            Quantity = (int)nudQuantity.Value,
            CoverPath = _coverPath
        });
        ClearForm();
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        var b = CurrentBook;
        if (b == null)
        {
            MessageBox.Show("Vui lòng chọn một cuốn sách trên lưới để sửa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ValidateInput(out long price)) return;

        // Model phát INotifyPropertyChanged => lưới và StatusStrip tự cập nhật
        b.Title = txtTitle.Text.Trim();
        b.Author = txtAuthor.Text.Trim();
        b.Price = price;
        b.Quantity = (int)nudQuantity.Value;
        b.CoverPath = _coverPath;
    }

    private void DeleteBook()
    {
        var b = CurrentBook;
        if (b == null)
        {
            MessageBox.Show("Vui lòng chọn một cuốn sách trên lưới để xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var r = MessageBox.Show($"Xóa sách \"{b.Title}\"?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (r != DialogResult.Yes) return;

        _books.Remove(b);
        ClearForm();
    }

    // ===================== TÌM KIẾM =====================
    private void mnuSearch_Click(object sender, EventArgs e)
    {
        string kw = PromptText("Tìm kiếm", "Nhập mã, tên sách hoặc tác giả:", _lastKeyword);
        if (string.IsNullOrEmpty(kw)) return;
        _lastKeyword = kw;

        int count = _books.Count;
        int start = dgvBooks.SelectedRows.Count > 0 ? dgvBooks.SelectedRows[0].Index + 1 : 0;

        // Tìm từ dòng kế tiếp, vòng lại đầu danh sách (nhấn Ctrl+F lần nữa để tìm tiếp)
        for (int i = 0; i < count; i++)
        {
            int idx = (start + i) % count;
            var b = _books[idx];
            if (Contains(b.Id, kw) || Contains(b.Title, kw) || Contains(b.Author, kw))
            {
                dgvBooks.ClearSelection();
                dgvBooks.Rows[idx].Selected = true;
                dgvBooks.CurrentCell = dgvBooks.Rows[idx].Cells[0];
                return;
            }
        }
        MessageBox.Show("Không tìm thấy kết quả phù hợp.", "Tìm kiếm",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static bool Contains(string s, string kw) =>
        s != null && s.Contains(kw, StringComparison.CurrentCultureIgnoreCase);

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
        var ok = new Button { Text = "Tìm", Left = 192, Top = 72, Width = 75, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Hủy", Left = 273, Top = 72, Width = 75, DialogResult = DialogResult.Cancel };
        f.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
        f.AcceptButton = ok;
        f.CancelButton = cancel;
        return f.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : null;
    }

    // ===================== XUẤT CSV (SaveFileDialog) =====================
    private void mnuSave_Click(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Lưu danh sách sách",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = "DanhSachSach.csv",
            OverwritePrompt = true
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine(CsvHeader);
            foreach (var b in _books)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(b.Id),
                    CsvEscape(b.Title),
                    CsvEscape(b.Author),
                    b.Price.ToString(CultureInfo.InvariantCulture),
                    b.Quantity.ToString(CultureInfo.InvariantCulture),
                    CsvEscape(b.CoverPath)));
            }
            // UTF-8 có BOM để Excel hiển thị đúng tiếng Việt
            File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));

            MessageBox.Show($"Đã lưu {_books.Count} đầu sách vào:\n{sfd.FileName}", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string CsvEscape(string s)
    {
        s ??= "";
        s = s.Replace("\r", " ").Replace("\n", " ");
        return s.IndexOfAny(new[] { ',', '"' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }

    // ===================== NHẬP CSV (OpenFileDialog) =====================
    private void mnuOpen_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Mở file danh sách sách",
            Filter = "CSV (*.csv)|*.csv",
            CheckFileExists = true
        };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var loaded = new List<BookModel>();
            int bad = 0;

            foreach (var line in File.ReadLines(ofd.FileName, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("MaSach", StringComparison.OrdinalIgnoreCase)) continue;  // dòng tiêu đề

                var f = ParseCsvLine(line);
                if (f.Count < 5 ||
                    !long.TryParse(f[3], NumberStyles.None, CultureInfo.InvariantCulture, out long price) ||
                    !int.TryParse(f[4], NumberStyles.None, CultureInfo.InvariantCulture, out int qty))
                {
                    bad++;
                    continue;
                }

                loaded.Add(new BookModel
                {
                    Id = f[0], Title = f[1], Author = f[2],
                    Price = price, Quantity = qty,
                    CoverPath = f.Count > 5 ? f[5] : ""
                });
            }

            // Nạp hàng loạt: tắt sự kiện, thêm xong rồi báo Reset 1 lần
            _books.RaiseListChangedEvents = false;
            _books.Clear();
            foreach (var b in loaded) _books.Add(b);
            _books.RaiseListChangedEvents = true;
            _books.ResetBindings();

            ClearForm();
            MessageBox.Show($"Đã nạp {loaded.Count} đầu sách." + (bad > 0 ? $"\nBỏ qua {bad} dòng sai định dạng." : ""),
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _books.RaiseListChangedEvents = true;
            MessageBox.Show("Không thể đọc file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Tách 1 dòng CSV, hỗ trợ trường nằm trong dấu nháy kép và "" là dấu nháy.</summary>
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == ',') { result.Add(sb.ToString().Trim()); sb.Clear(); }
            else sb.Append(c);
        }
        result.Add(sb.ToString().Trim());
        return result;
    }
}
