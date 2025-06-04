namespace BN.PROJECT.AlpacaService;

public class AlpacaQuote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Symbol { get; set; } = "";
    public DateTime TimestampUtc { get; set; }
    public string BidExchange { get; set; } = "";
    public string AskExchange { get; set; } = "";
    public decimal BidPrice { get; set; }
    public decimal AskPrice { get; set; }
    public decimal BidSize { get; set; }
    public decimal AskSize { get; set; }
    public string Tape { get; set; } = "";
}