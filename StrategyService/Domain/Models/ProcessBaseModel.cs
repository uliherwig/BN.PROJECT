namespace BN.PROJECT.StrategyService;

public class ProcessBaseModel
{
    public Guid StrategyId { get; set; }
    public bool IsBacktest { get; set; }
    public required string Asset { get; set; }
    public int Quantity { get; set; }
    public bool AllowOvernight { get; set; }
    public decimal TakeProfitPercent { get; set; }
    public decimal StopLossPercent { get; set; }
    public decimal TrailingStop { get; set; }
}
