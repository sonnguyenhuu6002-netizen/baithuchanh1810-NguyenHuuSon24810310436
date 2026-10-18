namespace BookManager;

partial class frmBookManager
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            picBookCover?.Image?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tmrClock = new System.Windows.Forms.Timer(components);

        // ---------- MenuStrip ----------
        menuStrip = new MenuStrip();
        mnuFile = new ToolStripMenuItem("&File");
        mnuOpen = new ToolStripMenuItem("Mở file CSV") { ShortcutKeys = Keys.Control | Keys.O };
        mnuSave = new ToolStripMenuItem("Lưu file CSV") { ShortcutKeys = Keys.Control | Keys.S };
        mnuExit = new ToolStripMenuItem("Thoát") { ShortcutKeys = Keys.Alt | Keys.F4 };
        mnuEdit = new ToolStripMenuItem("&Edit");
        mnuAdd = new ToolStripMenuItem("Thêm") { ShortcutKeys = Keys.Control | Keys.N };
        mnuDelete = new ToolStripMenuItem("Xóa") { ShortcutKeys = Keys.Control | Keys.D };
        mnuSearch = new ToolStripMenuItem("Tìm kiếm") { ShortcutKeys = Keys.Control | Keys.F };

        mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuOpen, mnuSave, new ToolStripSeparator(), mnuExit });
        mnuEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuAdd, mnuDelete, mnuSearch });
        menuStrip.Items.AddRange(new ToolStripItem[] { mnuFile, mnuEdit });
        mnuOpen.Click += mnuOpen_Click;
        mnuSave.Click += mnuSave_Click;
        mnuExit.Click += (s, e) => Close();
        mnuAdd.Click += (s, e) => AddBook();
        mnuDelete.Click += (s, e) => DeleteBook();
        mnuSearch.Click += mnuSearch_Click;

        // ---------- ToolStrip ----------
        toolStrip = new ToolStrip { ImageScalingSize = new Size(24, 24) };
        tsbOpen = new ToolStripButton("Mở file", IconFactory.Create("open")) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, ToolTipText = "Mở file CSV (Ctrl+O)" };
        tsbSave = new ToolStripButton("Lưu file", IconFactory.Create("save")) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, ToolTipText = "Lưu file CSV (Ctrl+S)" };
        tsbAdd = new ToolStripButton("Thêm mới", IconFactory.Create("add")) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, ToolTipText = "Thêm sách (Ctrl+N)" };
        toolStrip.Items.AddRange(new ToolStripItem[] { tsbOpen, tsbSave, new ToolStripSeparator(), tsbAdd });
        tsbOpen.Click += mnuOpen_Click;
        tsbSave.Click += mnuSave_Click;
        tsbAdd.Click += (s, e) => AddBook();

        // ---------- StatusStrip ----------
        statusStrip = new StatusStrip();
        lblCount = new ToolStripStatusLabel("Tổng số đầu sách: 0");
        lblTotal = new ToolStripStatusLabel("Tổng giá trị kho: 0") { BorderSides = ToolStripStatusLabelBorderSides.Left };
        lblSpring = new ToolStripStatusLabel { Spring = true };
        lblTime = new ToolStripStatusLabel("");
        statusStrip.Items.AddRange(new ToolStripItem[] { lblCount, lblTotal, lblSpring, lblTime });

        // ---------- Panel chính ----------
        pnlMain = new Panel { Dock = DockStyle.Fill };

        grpInfo = new GroupBox { Text = "Thông tin sách" };
        grpInfo.SetBounds(12, 8, 936, 215);
        grpInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblId = new Label { Text = "Mã sách:" }; lblId.SetBounds(15, 34, 90, 20);
        txtBookId = new TextBox { ReadOnly = true, TabStop = false }; txtBookId.SetBounds(110, 30, 260, 27);

        lblTitle = new Label { Text = "Tên sách:" }; lblTitle.SetBounds(15, 74, 90, 20);
        txtTitle = new TextBox(); txtTitle.SetBounds(110, 70, 260, 27);

        lblAuthor = new Label { Text = "Tác giả:" }; lblAuthor.SetBounds(15, 114, 90, 20);
        txtAuthor = new TextBox(); txtAuthor.SetBounds(110, 110, 260, 27);

        lblPrice = new Label { Text = "Giá (VNĐ):" }; lblPrice.SetBounds(400, 34, 90, 20);
        txtPrice = new TextBox(); txtPrice.SetBounds(495, 30, 210, 27);

        lblQty = new Label { Text = "Số lượng:" }; lblQty.SetBounds(400, 74, 90, 20);
        nudQuantity = new NumericUpDown { Minimum = 0, Maximum = 100000 }; nudQuantity.SetBounds(495, 70, 210, 27);

        lblCover = new Label { Text = "Ảnh bìa:" }; lblCover.SetBounds(400, 114, 90, 20);
        txtCoverFile = new TextBox { ReadOnly = true, TabStop = false }; txtCoverFile.SetBounds(495, 110, 210, 27);

        btnAdd = new Button { Text = "Thêm mới" }; btnAdd.SetBounds(110, 165, 110, 32);
        btnEdit = new Button { Text = "Sửa" }; btnEdit.SetBounds(230, 165, 110, 32);
        btnDelete = new Button { Text = "Xóa" }; btnDelete.SetBounds(350, 165, 110, 32);
        btnRefresh = new Button { Text = "Làm mới" }; btnRefresh.SetBounds(470, 165, 110, 32);
        btnAdd.Click += (s, e) => AddBook();
        btnEdit.Click += btnEdit_Click;
        btnDelete.Click += (s, e) => DeleteBook();
        btnRefresh.Click += (s, e) => ClearForm();

        picBookCover = new PictureBox { BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        picBookCover.SetBounds(760, 22, 150, 150);
        btnLoadCover = new Button { Text = "Tải ảnh bìa" };
        btnLoadCover.SetBounds(760, 176, 150, 30);
        btnLoadCover.Click += btnLoadCover_Click;

        grpInfo.Controls.AddRange(new Control[] {
            lblId, txtBookId, lblTitle, txtTitle, lblAuthor, txtAuthor,
            lblPrice, txtPrice, lblQty, nudQuantity, lblCover, txtCoverFile,
            btnAdd, btnEdit, btnDelete, btnRefresh, picBookCover, btnLoadCover });

        // Cột của lưới được tạo trong SetupGrid() bằng code
        dgvBooks = new DataGridView();
        dgvBooks.SetBounds(12, 232, 936, 300);
        dgvBooks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        pnlMain.Controls.AddRange(new Control[] { grpInfo, dgvBooks });

        // ---------- Form ----------
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(964, 640);
        MinimumSize = new Size(980, 600);
        // Thứ tự Add quan trọng để dock đúng: Fill trước, rồi Bottom/Top
        Controls.Add(pnlMain);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        Text = "Quản lý sách & xuất báo cáo nhà sách";
        StartPosition = FormStartPosition.CenterScreen;
        Load += frmBookManager_Load;
        tmrClock.Interval = 1000;
        tmrClock.Tick += (s, e) => UpdateClock();
    }

    private System.Windows.Forms.Timer tmrClock;
    private MenuStrip menuStrip;
    private ToolStripMenuItem mnuFile, mnuOpen, mnuSave, mnuExit, mnuEdit, mnuAdd, mnuDelete, mnuSearch;
    private ToolStrip toolStrip;
    private ToolStripButton tsbOpen, tsbSave, tsbAdd;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblCount, lblTotal, lblSpring, lblTime;
    private Panel pnlMain;
    private GroupBox grpInfo;
    private Label lblId, lblTitle, lblAuthor, lblPrice, lblQty, lblCover;
    private TextBox txtBookId, txtTitle, txtAuthor, txtPrice, txtCoverFile;
    private NumericUpDown nudQuantity;
    private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnLoadCover;
    private PictureBox picBookCover;
    private DataGridView dgvBooks;
}
