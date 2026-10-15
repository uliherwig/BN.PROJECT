namespace BN.PROJECT.AlpacaService;

public class OrderRequest
{
    public string Asset { get; set; } = "SPY";
    public decimal Price { get; set; } = 0;
    public decimal Quantity { get; set; } = 1;
    public SideEnum Side { get; set; } = SideEnum.Buy;
    public DateTime StampUtc { get; set; } = DateTime.UtcNow;

}


public class TestOrderRequest
{
    public string Asset { get; set; } = "SPY";
    public decimal Price { get; set; } = 0;
    public decimal Quantity { get; set; } = 1;
    public SideEnum Side { get; set; } = SideEnum.Buy;
    public DateTime StampUtc { get; set; } = DateTime.UtcNow;

}