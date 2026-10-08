namespace BN.PROJECT.AlpacaService;

// Tracks when a position was opened since Alpaca's position API exposes no entry timestamp.
public class AlpacaStrategyTracking
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? StoppedAtUtc { get; set; }
    public StrategyEnum StrategyType { get; set; } = StrategyEnum.NONE;
    public string Asset { get; set; } = string.Empty;
}
