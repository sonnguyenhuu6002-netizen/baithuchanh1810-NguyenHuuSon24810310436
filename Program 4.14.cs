namespace HospitalManager;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DataStore.Load();                       // nạp dữ liệu mẫu dùng chung cho các form
        Application.Run(new frmHospitalMain());
    }
}
