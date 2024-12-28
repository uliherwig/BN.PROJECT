namespace BN.PROJECT.StrategyService;

public class SmaTick
{
    public Guid StrategyId { get; set; }
    public decimal AskPrice { get; set; }
    public decimal BidPrice{ get; set; }
    public required string Asset { get; set; }
    public decimal ShortSma { get; set; }
    public decimal LongSma { get; set; }
    public DateTime TimestampUtc { get; set; }




}
