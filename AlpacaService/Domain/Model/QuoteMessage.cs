
namespace BN.PROJECT.AlpacaService;

internal class QuoteMessage
{
    public Guid UserId { get; set; }
    public string Symbol { get; set; }
    public decimal AskPrice { get; set; }
    public decimal BidPrice { get; set; }
    public DateTime TimestampUtc { get; set; }
}