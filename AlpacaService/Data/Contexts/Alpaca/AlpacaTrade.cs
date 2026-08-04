namespace BN.PROJECT.AlpacaService;

public class AlpacaTrade
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Symbol { get; set; } = "";          // z. B. "SPY"
    public DateTime TimestampUtc { get; set; }       // Zeitstempel des Trades (z. B. "2026-08-04T12:29:32.961151393Z")
    public string Exchange { get; set; } = "";        // Börse (z. B. "V" für NYSE, "Q" für NASDAQ)
    public decimal Price { get; set; }               // Ausführungspreis (z. B. 760.30)
    public decimal Size { get; set; }                // Ausgeführtes Volumen (Anzahl Aktien, z. B. 100)
    public string Tape { get; set; } = "";            // Tape (z. B. "A", "B", "C")
    public string[] Conditions { get; set; } = Array.Empty<string>(); // Handelsbedingungen (z. B. ["R"] für Regular Sale)
    public long TradeId { get; set; }                // Eindeutige Trade-ID (z. B. 12345)
}