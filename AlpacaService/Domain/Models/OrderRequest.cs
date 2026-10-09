namespace BN.PROJECT.AlpacaService;

public class OrderRequest
{
    public string Symbol { get; set; } = "SPY";

    public decimal Quantity { get; set; } = 1;

    public string Side { get; set; } = "Buy";

    public decimal StopLossPercent { get; set; } = 0;

    public decimal TakeProfitPercent { get; set; } = 0;   

}


public class TestOrderRequest
{
    public string Symbol { get; set; } = "SPY";

    public decimal Quantity { get; set; } = 1;

    public string Side { get; set; } = "Buy";

    public decimal StopLoss { get; set; } = 0;

    public decimal TakeProfit { get; set; } = 0;

    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;

}