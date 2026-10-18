namespace RevenueAnalytics;

public class TransactionModel
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Branch { get; set; }
    public string Item { get; set; }
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }

    // Thành tiền = số lượng x đơn giá
    public long Amount => Quantity * UnitPrice;
}
