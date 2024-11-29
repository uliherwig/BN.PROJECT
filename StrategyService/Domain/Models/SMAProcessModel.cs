namespace BN.PROJECT.StrategyService;

public class SMAProcessModel
{
    public Guid StrategyId { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan MarketCloseTime { get; set; }
    public required string Asset { get; set; }
    public int Quantity { get; set; }
    public bool IsIncreasing { get; set; }
    public bool AllowOvernight { get; set; }
    public decimal TakeProfitPercent { get; set; }
    public decimal StopLossPercent { get; set; }
    public decimal TrailingStop { get; set; }
    public int ShortPeriod { get; set; }
    public int LongPeriod { get; set; }
    public List<Quote> LastQuotes { get; set; } = [];




}
