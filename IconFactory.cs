using System.Drawing.Drawing2D;

namespace BookManager;

/// <summary>Vẽ icon nhỏ bằng GDI+ để không phụ thuộc file ảnh bên ngoài.</summary>
internal static class IconFactory
{
    public static Bitmap Create(string kind)
    {
        var bmp = new Bitmap(24, 24);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        switch (kind)
        {
            case "open":   // thư mục
                using (var b = new SolidBrush(Color.Goldenrod))
                {
                    g.FillRectangle(b, 2, 5, 9, 5);
                    g.FillRectangle(b, 2, 8, 20, 13);
                }
                using (var b = new SolidBrush(Color.Gold))
                    g.FillRectangle(b, 4, 11, 18, 10);
                break;

            case "save":   // đĩa mềm
                using (var b = new SolidBrush(Color.SteelBlue))
                    g.FillRectangle(b, 3, 3, 18, 18);
                using (var b = new SolidBrush(Color.White))
                    g.FillRectangle(b, 6, 3, 12, 7);
                using (var b = new SolidBrush(Color.LightGray))
                    g.FillRectangle(b, 7, 13, 10, 8);
                break;

            case "add":    // dấu cộng
                using (var b = new SolidBrush(Color.SeaGreen))
                    g.FillEllipse(b, 2, 2, 20, 20);
                using (var p = new Pen(Color.White, 3))
                {
                    g.DrawLine(p, 12, 6, 12, 18);
                    g.DrawLine(p, 6, 12, 18, 12);
                }
                break;
        }
        return bmp;
    }
}
