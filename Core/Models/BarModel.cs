namespace BN.PROJECT.Core;

public class BarModel
{
    public required string Asset { get; set; }    
    public DateTime TimestampUtc { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public int? NumberOfTrades { get; set; }
}