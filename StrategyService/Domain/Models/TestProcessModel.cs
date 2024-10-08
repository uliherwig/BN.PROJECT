namespace BN.PROJECT.StrategyService;

public class TestProcessModel
{
    public List<Position> Positions { get; set; } = [];
    public string? Symbol { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan TimeFrame { get; set; }
    public decimal DifferenceHighLow { get; set; }
    public bool IsIncreasing { get; set; }
    public decimal CurrentHigh { get; set; }
    public decimal CurrentLow { get; set; }
    public decimal PrevHigh { get; set; }
    public decimal PrevLow { get; set; }
    public long LastTimeFrameVolume { get; set; }
    public DateTime TimeFrameStart { get; set; }
    public DateTime PrevTimeFrameStart { get; set; }
    public decimal TakeProfitPlus { get; set; }
    public TimeSpan MarketCloseTime { get; set; }





}
