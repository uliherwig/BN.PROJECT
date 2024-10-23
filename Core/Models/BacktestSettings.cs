namespace BN.PROJECT.Core;

public class BacktestSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserEmail { get; set; }
    public required string Broker { get; set; } = "Alpaca";
    public required string Name { get; set; }
    public string Symbol { get; set; } = "SPY";
    public decimal TakeProfitPercent { get; set; } = 5m;
    public decimal StopLossPercent { get; set; } = 0m;
    public DateTime StartDate { get; set; } = DateTime.MinValue.ToUniversalTime();
    public DateTime EndDate { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public Strategy Strategy { get; set; } = Strategy.Breakout;
    public BreakoutPeriod BreakoutPeriod { get; set; } = BreakoutPeriod.Day;
    public decimal TrailingStop { get; set; } = 0m;
    public bool AllowOvernight { get; set; } = true;
    public bool Bookmarked { get; set; } = true;
    public StopLossStrategy StopLossStrategy { get; set; } = StopLossStrategy.Breakout;
    public DateTime TestStamp { get; set; } = DateTime.UtcNow.ToUniversalTime();
}
