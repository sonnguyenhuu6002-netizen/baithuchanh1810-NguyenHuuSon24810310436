namespace CryptoTicker;

/// <summary>Một dòng nhật ký biến động giá.</summary>
public class PriceLogModel
{
    public int Index { get; set; }
    public DateTime Time { get; set; }
    public decimal Price { get; set; }
    public decimal Change { get; set; }
    public double ChangePercent { get; set; }
    public string Trend { get; set; }
}
