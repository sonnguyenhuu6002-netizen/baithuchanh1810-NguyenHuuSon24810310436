using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BookManager;

public class BookModel : INotifyPropertyChanged
{
    private string _id;
    private string _title;
    private string _author;
    private long _price;
    private int _quantity;
    private string _coverPath = "";

    public string Id     { get => _id;     set => Set(ref _id, value); }
    public string Title  { get => _title;  set => Set(ref _title, value); }
    public string Author { get => _author; set => Set(ref _author, value); }

    public long Price
    {
        get => _price;
        set { if (Set(ref _price, value)) Raise(nameof(Total)); }
    }

    public int Quantity
    {
        get => _quantity;
        set { if (Set(ref _quantity, value)) Raise(nameof(Total)); }
    }

    // Đường dẫn file ảnh bìa (được lưu vào CSV)
    public string CoverPath
    {
        get => _coverPath;
        set { if (Set(ref _coverPath, value ?? "")) Raise(nameof(CoverFileName)); }
    }

    // Thuộc tính tính toán - dùng cho cột trên lưới
    public long Total => Price * Quantity;
    public string CoverFileName => string.IsNullOrEmpty(CoverPath) ? "" : Path.GetFileName(CoverPath);

    public event PropertyChangedEventHandler PropertyChanged;

    private void Raise(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        Raise(name);
        return true;
    }
}
