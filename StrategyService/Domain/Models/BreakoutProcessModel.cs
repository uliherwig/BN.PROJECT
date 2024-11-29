namespace BN.PROJECT.StrategyService;

public class BreakoutProcessModel
{
    public DateTime StartDate { get; set; }
    public TimeSpan BreakoutTimeSpan { get; set; }
    public long LastTimeFrameVolume { get; set; }
    public DateTime TimeFrameStart { get; set; }
    public DateTime PrevTimeFrameStart { get; set; }
    public decimal TakeProfitPlus { get; set; }
    public TimeSpan MarketCloseTime { get; set; }
    public decimal CurrentLow { get; set; }
    public DateTime CurrentLowStamp { get; set; }
    public decimal CurrentHigh { get; set; }
    public DateTime CurrentHighStamp { get; set; }
    public decimal PrevLow { get; set; }
    public DateTime PrevLowStamp { get; set; }
    public decimal PrevHigh { get; set; }
    public DateTime PrevHighStamp { get; set; }
    public required string Asset { get; set; }
    public int Quantity { get; set; }
    public TimeSpan TimeFrame { get; set; }
    public decimal DifferenceHighLow { get; set; }
    public bool IsIncreasing { get; set; }
    public bool AllowOvernight { get; set; }
    public decimal TakeProfitPercent { get; set; }
    public decimal StopLossPercent { get; set; }
    public decimal TrailingStop { get; set; }
    public Guid StrategyId { get; set; }



}
