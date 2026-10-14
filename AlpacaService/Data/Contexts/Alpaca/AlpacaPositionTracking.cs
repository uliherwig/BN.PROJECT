namespace BN.PROJECT.AlpacaService;

// Tracks when a position was opened since Alpaca's position API exposes no entry timestamp.
public class AlpacaPositionTracking
{
    [Key]
    public Guid Id { get; set; }

    public Guid StrategyId {get; set; }

    public string Asset { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public SideEnum Side { get; set; }

    public decimal OpenPrice  { get; set; }

    public decimal ClosePrice { get; set; } = 0;

    public DateTime OpenedAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; } = null;
}
