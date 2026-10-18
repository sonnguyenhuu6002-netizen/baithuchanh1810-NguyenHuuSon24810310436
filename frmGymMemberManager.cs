using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GymManager;

public partial class frmGymMemberManager : Form
{
    private readonly BindingList<MemberModel> _members = new();

    public frmGymMemberManager()
    {
        InitializeComponent();
    }

    // ===================== KHỞI TẠO =====================
    private void frmGymMemberManager_Load(object sender, EventArgs e)
    {
        cboPackage.Items.AddRange(new object[] { "Theo tháng", "6 tháng", "1 năm" });
        cboPackage.SelectedIndex = 0;

        SeedData();
        bsMembers.DataSource = _members;

        SetupGrid();
        SetupBindings();

        // Validate trực tiếp khi rời ô nhập
        txtFullName.Validating += (s, ev) => ValidateName();
        txtPhone.Validating += (s, ev) => ValidatePhone();
        txtFee.Validating += (s, ev) => ValidateFee();

        dgvMembers.SelectionChanged += dgvMembers_SelectionChanged;
    }

    private void frmGymMemberManager_Shown(object sender, EventArgs e) => ClearForm();

    private void SeedData()
    {
        _members.Add(new MemberModel { MemberId = "HV001", FullName = "Nguyễn Văn An", Phone = "0912345678", Package = "Theo tháng", StartDate = DateTime.Today.AddDays(-20), Fee = 300000 });
        _members.Add(new MemberModel { MemberId = "HV002", FullName = "Trần Thị Bình", Phone = "0987654321", Package = "6 tháng", StartDate = DateTime.Today.AddMonths(-2), Fee = 1500000 });
        _members.Add(new MemberModel { MemberId = "HV003", FullName = "Lê Hoàng Cường", Phone = "0901122334", Package = "1 năm", StartDate = DateTime.Today.AddMonths(-5), Fee = 2800000 });
    }

    // ===================== CẤU HÌNH GRID BẰNG CODE =====================
    private void SetupGrid()
    {
        dgvMembers.AutoGenerateColumns = false;   // bắt buộc: không tự sinh cột
        dgvMembers.Columns.Clear();
        dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMembers.MultiSelect = false;
        dgvMembers.ReadOnly = true;
        dgvMembers.AllowUserToAddRows = false;
        dgvMembers.AllowUserToDeleteRows = false;
        dgvMembers.RowHeadersVisible = false;
        dgvMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colMemberId", HeaderText = "Mã HV",
            DataPropertyName = nameof(MemberModel.MemberId), FillWeight = 70
        });
        dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colFullName", HeaderText = "Tên hội viên",
            DataPropertyName = nameof(MemberModel.FullName), FillWeight = 160
        });
        dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPhone", HeaderText = "SĐT",
            DataPropertyName = nameof(MemberModel.Phone), FillWeight = 100
        });
        dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPackage", HeaderText = "Gói tập",
            DataPropertyName = nameof(MemberModel.Package), FillWeight = 90
        });
        dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colStartDate", HeaderText = "Ngày ĐK",
            DataPropertyName = nameof(MemberModel.StartDate), FillWeight = 90,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
        });
        dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colFee", HeaderText = "Phí dịch vụ",
            DataPropertyName = nameof(MemberModel.Fee), FillWeight = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N0",
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        });

        dgvMembers.DataSource = bsMembers;
    }

    // ===================== DATA BINDING =====================
    // DataSourceUpdateMode.Never: control chỉ HIỂN THỊ dữ liệu của dòng đang chọn,
    // người dùng gõ dở sẽ KHÔNG làm đổi dữ liệu gốc. Chỉ khi bấm "Sửa" mới ghi xuống model.
    private void SetupBindings()
    {
        txtMemberId.DataBindings.Add("Text", bsMembers, nameof(MemberModel.MemberId), false, DataSourceUpdateMode.Never);
        txtFullName.DataBindings.Add("Text", bsMembers, nameof(MemberModel.FullName), false, DataSourceUpdateMode.Never);
        txtPhone.DataBindings.Add("Text", bsMembers, nameof(MemberModel.Phone), false, DataSourceUpdateMode.Never);
        cboPackage.DataBindings.Add("Text", bsMembers, nameof(MemberModel.Package), false, DataSourceUpdateMode.Never);
        dtpStartDate.DataBindings.Add("Value", bsMembers, nameof(MemberModel.StartDate), false, DataSourceUpdateMode.Never);
        txtFee.DataBindings.Add("Text", bsMembers, nameof(MemberModel.Fee), false, DataSourceUpdateMode.Never);
    }

    // Chọn dòng (kể cả click lại đúng dòng đang chọn) => đẩy dữ liệu lên control
    private void dgvMembers_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvMembers.SelectedRows.Count == 0) return;
        bsMembers.Position = dgvMembers.SelectedRows[0].Index;
        bsMembers.ResetCurrentItem();   // buộc binding nạp lại giá trị vào control
        errorProvider.Clear();
    }

    // ===================== VALIDATE (ErrorProvider) =====================
    private bool ValidateName()
    {
        string name = txtFullName.Text.Trim();
        if (name.Length == 0)
        {
            errorProvider.SetError(txtFullName, "Tên hội viên không được để trống.");
            return false;
        }
        // Chỉ cho phép chữ cái (có dấu tiếng Việt) và khoảng trắng
        if (!Regex.IsMatch(name, @"^[\p{L}\s]+$"))
        {
            errorProvider.SetError(txtFullName, "Tên không được chứa số hoặc ký tự đặc biệt.");
            return false;
        }
        errorProvider.SetError(txtFullName, "");
        return true;
    }

    private bool ValidatePhone()
    {
        if (!Regex.IsMatch(txtPhone.Text.Trim(), @"^\d{10}$"))
        {
            errorProvider.SetError(txtPhone, "Số điện thoại phải gồm đúng 10 chữ số.");
            return false;
        }
        errorProvider.SetError(txtPhone, "");
        return true;
    }

    private bool ValidateFee()
    {
        string text = txtFee.Text.Trim();
        if (!Regex.IsMatch(text, @"^\d+$") ||
            !int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int fee) || fee <= 0)
        {
            errorProvider.SetError(txtFee, "Phí dịch vụ phải là số nguyên dương.");
            return false;
        }
        errorProvider.SetError(txtFee, "");
        return true;
    }

    private bool ValidateAll()
    {
        // Dùng & (không phải &&) để hiện lỗi của TẤT CẢ ô cùng lúc
        return ValidateName() & ValidatePhone() & ValidateFee();
    }

    // ===================== CHỨC NĂNG =====================
    private string GenerateId()
    {
        int max = _members
            .Select(m => int.TryParse(m.MemberId.AsSpan(2), out int n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"HV{max + 1:D3}";
    }

    private void ClearForm()
    {
        dgvMembers.ClearSelection();
        errorProvider.Clear();
        txtMemberId.Text = GenerateId();
        txtFullName.Clear();
        txtPhone.Clear();
        txtFee.Clear();
        cboPackage.SelectedIndex = 0;
        dtpStartDate.Value = DateTime.Today;
        txtFullName.Focus();
    }

    private void btnRefresh_Click(object sender, EventArgs e) => ClearForm();

    private void btnAdd_Click(object sender, EventArgs e)
    {
        if (!ValidateAll()) return;

        _members.Add(new MemberModel
        {
            MemberId = GenerateId(),
            FullName = NormalizeName(txtFullName.Text),
            Phone = txtPhone.Text.Trim(),
            Package = cboPackage.Text,
            StartDate = dtpStartDate.Value.Date,
            Fee = int.Parse(txtFee.Text.Trim(), CultureInfo.InvariantCulture)
        });

        MessageBox.Show("Thêm hội viên thành công!", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        ClearForm();
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        if (bsMembers.Current is not MemberModel member || dgvMembers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn hội viên cần sửa trên lưới.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ValidateAll()) return;

        // Ghi từ control xuống BindingList<T>. Model phát INotifyPropertyChanged
        // => BindingList báo ListChanged => DataGridView tự vẽ lại dòng, KHÔNG cần nạp lại danh sách.
        member.FullName = NormalizeName(txtFullName.Text);
        member.Phone = txtPhone.Text.Trim();
        member.Package = cboPackage.Text;
        member.StartDate = dtpStartDate.Value.Date;
        member.Fee = int.Parse(txtFee.Text.Trim(), CultureInfo.InvariantCulture);

        MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (bsMembers.Current is not MemberModel member || dgvMembers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn hội viên cần xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc muốn xóa hội viên {member.MemberId} - {member.FullName}?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (result != DialogResult.Yes) return;

        bsMembers.RemoveCurrent();
        ClearForm();
    }

    // Chuẩn hóa khoảng trắng thừa: "  nguyễn   văn  an " -> "nguyễn văn an"
    private static string NormalizeName(string s) =>
        Regex.Replace(s.Trim(), @"\s+", " ");
}
