namespace BN.PROJECT.Core;

public class BacktestSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string Symbol { get; set; } = "SPY";
    public double TakeProfitFactor { get; set; } = 0.01;
    public double StopLossFactor { get; set; } = 0.01;
    public DateTime StartDate { get; set; } = DateTime.MinValue.ToUniversalTime();
    public DateTime EndDate { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public Strategy Strategy { get; set; } = Strategy.Breakout;
    public TradeInterval TimeFrame { get; set; } = TradeInterval.Day;
    public bool AllowOvernight { get; set; } = true;
    public required string UserEmail { get; set; }
    public DateTime TestStamp { get; set; } = DateTime.UtcNow.ToUniversalTime();
}
