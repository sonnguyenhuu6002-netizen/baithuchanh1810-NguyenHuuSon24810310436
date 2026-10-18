namespace HospitalManager;

/// <summary>Form con 3: Danh mục Khoa/Phòng.</summary>
public class frmDepartments : Form, IReloadable
{
    private readonly DataGridView dgv = new();

    public frmDepartments()
    {
        Text = "Danh mục Khoa/Phòng";
        ClientSize = new Size(640, 360);

        UiHelper.ConfigGrid(dgv);
        UiHelper.AddCol(dgv, "colId", "Mã khoa", nameof(Department.Id), 60);
        UiHelper.AddCol(dgv, "colName", "Tên khoa/phòng", nameof(Department.Name), 150);
        UiHelper.AddCol(dgv, "colBeds", "Số giường", nameof(Department.Beds), 70);
        UiHelper.AddCol(dgv, "colLoc", "Vị trí", nameof(Department.Location), 130);

        dgv.Dock = DockStyle.Fill;
        dgv.DataSource = DataStore.Departments;
        Controls.Add(dgv);
    }

    public void ReloadData() => dgv.Refresh();
}
