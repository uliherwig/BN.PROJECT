namespace BN.PROJECT.Core;

public class BacktestSettings
{
    public string Symbol { get; set; } = "SPY";
    public double TakeProfitFactor { get; set; } = 0.01;
    public double StopLossFactor { get; set; } = 0.01;   
    public DateTime StartDate { get; set; } = DateTime.Now.AddYears(-1);
    public DateTime EndDate { get; set; } = DateTime.Now;
    public Strategy Strategy { get; set; } = Strategy.Breakout;
    public TimeFrame TimeFrame { get; set; } = TimeFrame.Day; 
    public required string UserEmail { get; set; }
}
