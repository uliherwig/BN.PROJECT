namespace BN.PROJECT.AlpacaService;

// Tracks when a position was opened since Alpaca's position API exposes no entry timestamp.
public class AlpacaPositionTracking
{
    [Key]
    public Guid Id { get; set; }

    public string Symbol { get; set; } = string.Empty;

    public DateTime OpenedAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }
}
