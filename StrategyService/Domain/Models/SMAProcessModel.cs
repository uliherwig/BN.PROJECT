namespace BN.PROJECT.StrategyService;

public class SmaProcessModel : ProcessBaseModel
{

    public DateTime StartDate { get; set; }
    public TimeSpan MarketCloseTime { get; set; }
    public int ShortPeriod { get; set; }
    public int LongPeriod { get; set; }
    public StopLossTypeEnum StopLossType { get; set; }
    public List<Quote> LastQuotes { get; set; } = [];
    public List<SmaTick> LastSmas { get; set; } = [];
}