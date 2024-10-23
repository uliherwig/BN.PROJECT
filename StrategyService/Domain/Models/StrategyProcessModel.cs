namespace BN.PROJECT.StrategyService;

public class StrategyProcessModel
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



}
