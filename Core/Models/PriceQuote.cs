using Humanizer;

namespace BN.PROJECT.Core;

public class PriceQuote
{
    public string Symbol { get; set; }
    public decimal BidPrice { get; set; }
    public decimal AskPrice { get; set; }
    public DateTime TimestampUtc { get; set; }
    public decimal Volume  { get; set; }
}