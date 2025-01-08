namespace BN.PROJECT.StrategyService;

public class SmaProcessModel
{
    public Guid StrategyId { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan MarketCloseTime { get; set; }
    public string Asset { get; set; }
    public int Quantity { get; set; }
    public bool IsIncreasing { get; set; }
    public bool AllowOvernight { get; set; }
    public decimal TakeProfitPercent { get; set; }
    public decimal StopLossPercent { get; set; }
    public decimal TrailingStop { get; set; }
    public int ShortPeriod { get; set; }
    public int LongPeriod { get; set; }
    public StopLossTypeEnum StopLossType { get; set; }
    public decimal IntersectionThreshold { get; set; }
    public List<Quote> LastQuotes { get; set; } = [];
    public List<SmaTick> LastSmas { get; set; } = [];
}