namespace GymManager;

partial class frmGymMemberManager
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        errorProvider = new ErrorProvider(components);
        bsMembers = new BindingSource(components);

        grpInfo = new GroupBox();
        lblMemberId = new Label(); txtMemberId = new TextBox();
        lblFullName = new Label(); txtFullName = new TextBox();
        lblPhone = new Label(); txtPhone = new TextBox();
        lblPackage = new Label(); cboPackage = new ComboBox();
        lblStartDate = new Label(); dtpStartDate = new DateTimePicker();
        lblFee = new Label(); txtFee = new TextBox();

        grpActions = new GroupBox();
        btnAdd = new Button(); btnEdit = new Button();
        btnDelete = new Button(); btnRefresh = new Button();

        dgvMembers = new DataGridView();

        ((System.ComponentModel.ISupportInitialize)errorProvider).BeginInit();
        ((System.ComponentModel.ISupportInitialize)bsMembers).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvMembers).BeginInit();
        SuspendLayout();

        // errorProvider
        errorProvider.ContainerControl = this;
        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        // grpInfo
        grpInfo.Text = "Thông tin hội viên";
        grpInfo.SetBounds(12, 12, 836, 150);
        grpInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblMemberId.Text = "Mã hội viên:"; lblMemberId.SetBounds(15, 34, 100, 20);
        txtMemberId.Name = "txtMemberId"; txtMemberId.SetBounds(120, 30, 250, 27);
        txtMemberId.ReadOnly = true; txtMemberId.TabStop = false;

        lblFullName.Text = "Họ và tên:"; lblFullName.SetBounds(15, 74, 100, 20);
        txtFullName.Name = "txtFullName"; txtFullName.SetBounds(120, 70, 250, 27);

        lblPhone.Text = "Số điện thoại:"; lblPhone.SetBounds(15, 114, 100, 20);
        txtPhone.Name = "txtPhone"; txtPhone.SetBounds(120, 110, 250, 27);
        txtPhone.MaxLength = 10;

        lblPackage.Text = "Gói tập:"; lblPackage.SetBounds(430, 34, 90, 20);
        cboPackage.Name = "cboPackage"; cboPackage.SetBounds(525, 30, 280, 28);
        cboPackage.DropDownStyle = ComboBoxStyle.DropDownList;

        lblStartDate.Text = "Ngày đăng ký:"; lblStartDate.SetBounds(430, 74, 95, 20);
        dtpStartDate.Name = "dtpStartDate"; dtpStartDate.SetBounds(525, 70, 280, 27);
        dtpStartDate.Format = DateTimePickerFormat.Custom;
        dtpStartDate.CustomFormat = "dd/MM/yyyy";

        lblFee.Text = "Phí dịch vụ:"; lblFee.SetBounds(430, 114, 90, 20);
        txtFee.Name = "txtFee"; txtFee.SetBounds(525, 110, 280, 27);

        grpInfo.Controls.AddRange(new Control[] {
            lblMemberId, txtMemberId, lblFullName, txtFullName, lblPhone, txtPhone,
            lblPackage, cboPackage, lblStartDate, dtpStartDate, lblFee, txtFee });

        // grpActions
        grpActions.Text = "Chức năng";
        grpActions.SetBounds(12, 170, 836, 64);
        grpActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        btnAdd.Name = "btnAdd"; btnAdd.Text = "Thêm mới"; btnAdd.SetBounds(15, 24, 150, 30);
        btnEdit.Name = "btnEdit"; btnEdit.Text = "Sửa thông tin"; btnEdit.SetBounds(185, 24, 150, 30);
        btnDelete.Name = "btnDelete"; btnDelete.Text = "Xóa hội viên"; btnDelete.SetBounds(355, 24, 150, 30);
        btnRefresh.Name = "btnRefresh"; btnRefresh.Text = "Làm mới"; btnRefresh.SetBounds(525, 24, 150, 30);
        btnAdd.Click += btnAdd_Click;
        btnEdit.Click += btnEdit_Click;
        btnDelete.Click += btnDelete_Click;
        btnRefresh.Click += btnRefresh_Click;
        grpActions.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

        // dgvMembers (KHÔNG khai báo cột ở đây - cột được tạo bằng code trong form .cs)
        dgvMembers.Name = "dgvMembers";
        dgvMembers.SetBounds(12, 244, 836, 304);
        dgvMembers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        // Form
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(860, 560);
        Controls.AddRange(new Control[] { grpInfo, grpActions, dgvMembers });
        Name = "frmGymMemberManager";
        Text = "Quản lý hội viên phòng Gym";
        StartPosition = FormStartPosition.CenterScreen;
        Load += frmGymMemberManager_Load;
        Shown += frmGymMemberManager_Shown;

        ((System.ComponentModel.ISupportInitialize)errorProvider).EndInit();
        ((System.ComponentModel.ISupportInitialize)bsMembers).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvMembers).EndInit();
        ResumeLayout(false);
    }

    private ErrorProvider errorProvider;
    private BindingSource bsMembers;
    private GroupBox grpInfo, grpActions;
    private Label lblMemberId, lblFullName, lblPhone, lblPackage, lblStartDate, lblFee;
    private TextBox txtMemberId, txtFullName, txtPhone, txtFee;
    private ComboBox cboPackage;
    private DateTimePicker dtpStartDate;
    private Button btnAdd, btnEdit, btnDelete, btnRefresh;
    private DataGridView dgvMembers;
}
