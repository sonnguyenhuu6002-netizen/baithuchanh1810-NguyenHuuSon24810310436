namespace HospitalManager;

/// <summary>Các hàm dựng control bằng code để các form con gọn hơn.</summary>
internal static class UiHelper
{
    public static Label Lbl(string text, int x, int y, int w = 90) =>
        new Label { Text = text, Left = x, Top = y + 4, Width = w, AutoSize = false };

    public static Button Btn(string text, int x, int y, int w = 110) =>
        new Button { Text = text, Left = x, Top = y, Width = w, Height = 32 };

    public static void ConfigGrid(DataGridView dgv)
    {
        dgv.AutoGenerateColumns = false;        // không tự sinh cột
        dgv.Columns.Clear();
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.RowHeadersVisible = false;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }

    public static void AddCol(DataGridView dgv, string name, string header, string prop, float weight, string format = null)
    {
        var col = new DataGridViewTextBoxColumn
        {
            Name = name, HeaderText = header, DataPropertyName = prop, FillWeight = weight
        };
        if (format != null) col.DefaultCellStyle.Format = format;
        dgv.Columns.Add(col);
    }
}
