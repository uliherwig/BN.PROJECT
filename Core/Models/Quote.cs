namespace BN.PROJECT.Core;

public class Quote
{
    public required string Symbol { get; set; }
    public decimal BidPrice { get; set; }
    public decimal AskPrice { get; set; }
    public DateTime TimestampUtc { get; set; }
}

