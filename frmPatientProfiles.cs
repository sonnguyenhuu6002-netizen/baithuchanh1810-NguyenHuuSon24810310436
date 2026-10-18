using System.Text.RegularExpressions;

namespace HospitalManager;

/// <summary>Form con 1: Hồ sơ bệnh nhân.</summary>
public class frmPatientProfiles : Form, IReloadable
{
    private readonly DataGridView dgv = new();
    private TextBox txtId, txtName, txtDiagnosis;
    private DateTimePicker dtpBirth;
    private ComboBox cboGender, cboDept;
    private Button btnAdd, btnEdit, btnDelete, btnRefresh;

    public frmPatientProfiles()
    {
        BuildUi();
        SetupGrid();
        LoadDepartments();

        dgv.DataSource = DataStore.Patients;
        dgv.SelectionChanged += dgv_SelectionChanged;
        ClearForm();
    }

    private void BuildUi()
    {
        Text = "Hồ sơ Bệnh nhân";
        ClientSize = new Size(820, 480);

        var grp = new GroupBox { Text = "Thông tin bệnh nhân", Dock = DockStyle.Top, Height = 190 };

        txtId = new TextBox { Left = 110, Top = 30, Width = 250, ReadOnly = true, TabStop = false };
        txtName = new TextBox { Left = 110, Top = 65, Width = 250 };
        dtpBirth = new DateTimePicker
        {
            Left = 110, Top = 100, Width = 250,
            Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", MaxDate = DateTime.Today
        };

        cboGender = new ComboBox { Left = 500, Top = 30, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        cboGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
        cboDept = new ComboBox { Left = 500, Top = 65, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
        txtDiagnosis = new TextBox { Left = 500, Top = 100, Width = 250 };

        btnAdd = UiHelper.Btn("Thêm mới", 110, 140);
        btnEdit = UiHelper.Btn("Sửa", 230, 140);
        btnDelete = UiHelper.Btn("Xóa", 350, 140);
        btnRefresh = UiHelper.Btn("Làm mới", 470, 140);
        btnAdd.Click += btnAdd_Click;
        btnEdit.Click += btnEdit_Click;
        btnDelete.Click += btnDelete_Click;
        btnRefresh.Click += (s, e) => ClearForm();

        grp.Controls.AddRange(new Control[] {
            UiHelper.Lbl("Mã BN:", 15, 30), txtId,
            UiHelper.Lbl("Họ và tên:", 15, 65), txtName,
            UiHelper.Lbl("Ngày sinh:", 15, 100), dtpBirth,
            UiHelper.Lbl("Giới tính:", 405, 30), cboGender,
            UiHelper.Lbl("Khoa:", 405, 65), cboDept,
            UiHelper.Lbl("Chẩn đoán:", 405, 100), txtDiagnosis,
            btnAdd, btnEdit, btnDelete, btnRefresh });

        // Thứ tự Add: Fill trước, Top sau để dock không chồng lên nhau
        dgv.Dock = DockStyle.Fill;
        Controls.Add(dgv);
        Controls.Add(grp);
    }

    private void SetupGrid()
    {
        UiHelper.ConfigGrid(dgv);
        UiHelper.AddCol(dgv, "colId", "Mã BN", nameof(Patient.Id), 60);
        UiHelper.AddCol(dgv, "colName", "Họ và tên", nameof(Patient.FullName), 150);
        UiHelper.AddCol(dgv, "colBirth", "Ngày sinh", nameof(Patient.BirthDate), 80, "dd/MM/yyyy");
        UiHelper.AddCol(dgv, "colGender", "Giới tính", nameof(Patient.Gender), 60);
        UiHelper.AddCol(dgv, "colDept", "Khoa", nameof(Patient.Department), 100);
        UiHelper.AddCol(dgv, "colDiag", "Chẩn đoán", nameof(Patient.Diagnosis), 150);
    }

    private void LoadDepartments()
    {
        cboDept.Items.Clear();
        foreach (var d in DataStore.Departments) cboDept.Items.Add(d.Name);
    }

    // Được form cha gọi khi bấm "Nạp lại dữ liệu"
    public void ReloadData()
    {
        LoadDepartments();
        ClearForm();
    }

    private Patient Current =>
        dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0].DataBoundItem as Patient : null;

    private void dgv_SelectionChanged(object sender, EventArgs e)
    {
        var p = Current;
        if (p == null) return;

        txtId.Text = p.Id;
        txtName.Text = p.FullName;
        dtpBirth.Value = p.BirthDate > dtpBirth.MaxDate ? dtpBirth.MaxDate : p.BirthDate;
        cboGender.Text = p.Gender;
        cboDept.Text = p.Department;
        txtDiagnosis.Text = p.Diagnosis;
    }

    private void ClearForm()
    {
        dgv.ClearSelection();
        txtId.Text = NextId();
        txtName.Clear();
        dtpBirth.Value = DateTime.Today;
        cboGender.SelectedIndex = 0;
        cboDept.SelectedIndex = cboDept.Items.Count > 0 ? 0 : -1;
        txtDiagnosis.Clear();
        txtName.Focus();
    }

    private static string NextId()
    {
        int max = DataStore.Patients
            .Select(p => p.Id != null && p.Id.Length > 2 && int.TryParse(p.Id.Substring(2), out int n) ? n : 0)
            .DefaultIfEmpty(0).Max();
        return $"BN{max + 1:D3}";
    }

    private bool ValidateInput()
    {
        string name = txtName.Text.Trim();
        if (name.Length == 0 || !Regex.IsMatch(name, @"^[\p{L}\s]+$"))
        {
            MessageBox.Show("Họ tên không được trống và không chứa số/ký tự đặc biệt.", "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtName.Focus();
            return false;
        }
        if (cboDept.SelectedIndex < 0)
        {
            MessageBox.Show("Vui lòng chọn khoa điều trị.", "Dữ liệu không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        if (!ValidateInput()) return;

        DataStore.Patients.Add(new Patient
        {
            Id = NextId(),
            FullName = Regex.Replace(txtName.Text.Trim(), @"\s+", " "),
            BirthDate = dtpBirth.Value.Date,
            Gender = cboGender.Text,
            Department = cboDept.Text,
            Diagnosis = txtDiagnosis.Text.Trim()
        });
        ClearForm();
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        var p = Current;
        if (p == null)
        {
            MessageBox.Show("Vui lòng chọn bệnh nhân cần sửa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ValidateInput()) return;

        p.FullName = Regex.Replace(txtName.Text.Trim(), @"\s+", " ");
        p.BirthDate = dtpBirth.Value.Date;
        p.Gender = cboGender.Text;
        p.Department = cboDept.Text;
        p.Diagnosis = txtDiagnosis.Text.Trim();

        // Patient là class thuần (không INotifyPropertyChanged) nên báo lưới vẽ lại dòng này
        DataStore.Patients.ResetItem(DataStore.Patients.IndexOf(p));
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        var p = Current;
        if (p == null)
        {
            MessageBox.Show("Vui lòng chọn bệnh nhân cần xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var r = MessageBox.Show($"Xóa hồ sơ bệnh nhân {p.Id} - {p.FullName}?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (r != DialogResult.Yes) return;

        DataStore.Patients.Remove(p);
        ClearForm();
    }
}
