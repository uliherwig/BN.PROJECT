namespace BN.PROJECT.AlpacaService;

// Represents a quote for a specific symbol at a given timestamp, including bid and ask prices and sizes.
// Includes information about the tape from which the quote originated.
// A (NYSE)
// B (NASDAQ)
// C (NYSE ARCA)
// etc.
// Tape codes indicate the exchange from which the quote originated.

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