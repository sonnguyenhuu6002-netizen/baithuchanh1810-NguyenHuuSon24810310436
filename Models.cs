using System.ComponentModel;

namespace HospitalManager;

/// <summary>Form con nào cần cập nhật lại khi "Nạp lại dữ liệu" thì implement interface này.</summary>
public interface IReloadable
{
    void ReloadData();
}

public class Patient
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; }
    public string Department { get; set; }
    public string Diagnosis { get; set; }
}

public class Department
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Beds { get; set; }
    public string Location { get; set; }
}

public class DutyShift
{
    public DateTime Date { get; set; }
    public string Shift { get; set; }
    public string DoctorName { get; set; }
    public string Department { get; set; }
}

/// <summary>Kho dữ liệu mẫu dùng chung (static) cho toàn bộ form con.</summary>
public static class DataStore
{
    public static BindingList<Patient> Patients { get; } = new();
    public static BindingList<Department> Departments { get; } = new();
    public static BindingList<DutyShift> Shifts { get; } = new();

    public static void Load()
    {
        Departments.Clear();
        Departments.Add(new Department { Id = "K01", Name = "Nội tổng hợp", Beds = 30, Location = "Tòa A - Tầng 2" });
        Departments.Add(new Department { Id = "K02", Name = "Ngoại khoa", Beds = 25, Location = "Tòa A - Tầng 3" });
        Departments.Add(new Department { Id = "K03", Name = "Nhi khoa", Beds = 20, Location = "Tòa B - Tầng 1" });
        Departments.Add(new Department { Id = "K04", Name = "Sản khoa", Beds = 15, Location = "Tòa B - Tầng 2" });
        Departments.Add(new Department { Id = "K05", Name = "Cấp cứu", Beds = 10, Location = "Tòa C - Tầng 1" });

        Patients.Clear();
        Patients.Add(new Patient { Id = "BN001", FullName = "Nguyễn Văn An", BirthDate = new DateTime(1985, 3, 12), Gender = "Nam", Department = "Nội tổng hợp", Diagnosis = "Viêm dạ dày" });
        Patients.Add(new Patient { Id = "BN002", FullName = "Trần Thị Bình", BirthDate = new DateTime(1992, 7, 25), Gender = "Nữ", Department = "Sản khoa", Diagnosis = "Theo dõi thai kỳ" });
        Patients.Add(new Patient { Id = "BN003", FullName = "Lê Minh Cường", BirthDate = new DateTime(2016, 11, 2), Gender = "Nam", Department = "Nhi khoa", Diagnosis = "Viêm họng cấp" });
        Patients.Add(new Patient { Id = "BN004", FullName = "Phạm Thu Dung", BirthDate = new DateTime(1970, 1, 30), Gender = "Nữ", Department = "Ngoại khoa", Diagnosis = "Sỏi mật" });
        Patients.Add(new Patient { Id = "BN005", FullName = "Hoàng Quốc Em", BirthDate = new DateTime(1998, 9, 9), Gender = "Nam", Department = "Cấp cứu", Diagnosis = "Gãy xương cẳng tay" });

        Shifts.Clear();
        string[] doctors = { "BS. Nguyễn Văn Hùng", "BS. Trần Thị Lan", "BS. Lê Quang Minh", "BS. Phạm Thu Hà" };
        string[] shifts = { "Sáng (07:00 - 13:00)", "Chiều (13:00 - 19:00)", "Đêm (19:00 - 07:00)" };
        for (int d = 0; d < 7; d++)
        {
            for (int s = 0; s < shifts.Length; s++)
            {
                Shifts.Add(new DutyShift
                {
                    Date = DateTime.Today.AddDays(d),
                    Shift = shifts[s],
                    DoctorName = doctors[(d + s) % doctors.Length],
                    Department = Departments[(d + s) % Departments.Count].Name
                });
            }
        }
    }
}
