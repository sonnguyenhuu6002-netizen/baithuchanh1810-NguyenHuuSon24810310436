using System.ComponentModel;

namespace HospitalManager;

/// <summary>Form con 2: Lịch trực của Bác sĩ.</summary>
public class frmDoctorSchedule : Form, IReloadable
{
    private readonly DataGridView dgv = new();
    private DateTimePicker dtpDate;
    private Button btnFilter, btnAll;
    private DateTime? _filterDate;     // null = xem tất cả

    public frmDoctorSchedule()
    {
        BuildUi();
        UiHelper.ConfigGrid(dgv);
        UiHelper.AddCol(dgv, "colDate", "Ngày trực", nameof(DutyShift.Date), 70, "dd/MM/yyyy");
        UiHelper.AddCol(dgv, "colShift", "Ca trực", nameof(DutyShift.Shift), 100);
        UiHelper.AddCol(dgv, "colDoctor", "Bác sĩ", nameof(DutyShift.DoctorName), 130);
        UiHelper.AddCol(dgv, "colDept", "Khoa", nameof(DutyShift.Department), 100);
        ApplyFilter();
    }

    private void BuildUi()
    {
        Text = "Lịch trực của Bác sĩ";
        ClientSize = new Size(720, 440);

        var pnl = new Panel { Dock = DockStyle.Top, Height = 52 };
        dtpDate = new DateTimePicker
        {
            Left = 130, Top = 12, Width = 140,
            Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy"
        };
        btnFilter = UiHelper.Btn("Lọc theo ngày", 285, 8, 130);
        btnAll = UiHelper.Btn("Xem tất cả", 425, 8, 120);
        btnFilter.Click += (s, e) => { _filterDate = dtpDate.Value.Date; ApplyFilter(); };
        btnAll.Click += (s, e) => { _filterDate = null; ApplyFilter(); };
        pnl.Controls.AddRange(new Control[] { UiHelper.Lbl("Xem lịch ngày:", 12, 12, 115), dtpDate, btnFilter, btnAll });

        dgv.Dock = DockStyle.Fill;
        Controls.Add(dgv);
        Controls.Add(pnl);
    }

    private void ApplyFilter()
    {
        var data = DataStore.Shifts
            .Where(s => _filterDate == null || s.Date.Date == _filterDate.Value)
            .ToList();
        dgv.DataSource = new BindingList<DutyShift>(data);
    }

    public void ReloadData() => ApplyFilter();
}
