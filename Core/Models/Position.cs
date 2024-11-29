namespace BN.PROJECT.Core;

public class Position
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StrategyId { get; set; }
    public required string Symbol { get; set; }
    public int Quantity { get; set; }
    public SideEnum Side { get; set; }
    public decimal PriceOpen { get; set; }
    public decimal PriceClose { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal StopLoss { get; set; }
    public DateTime StampOpened { get; set; }
    public DateTime StampClosed { get; set; }
    public string CloseSignal { get; set; } = string.Empty;
    public StrategyEnum StrategyType { get; set; }
    public string StrategyParams { get; set; } = string.Empty;
   
}
