namespace BN.PROJECT.AlpacaService;

public class OrderRequest
{
    public string Symbol { get; set; } = "SPY";

    public decimal Quantity { get; set; } = 1;

    public string Side { get; set; } = "Buy";

    public string OrderType { get; set; } = "Market";

    public string TimeInForce { get; set; } = "Day";

}


public class TestOrderRequest
{
    public string Symbol { get; set; } = "SPY";

    public decimal Quantity { get; set; } = 1;

    public string Side { get; set; } = "Buy";

    public string OrderType { get; set; } = "Market";

    public string TimeInForce { get; set; } = "Day";

    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;

}