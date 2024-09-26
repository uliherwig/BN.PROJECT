namespace BN.PROJECT.AlpacaService;

public class BacktestSettings
{
    public string Symbol { get; set; } = "SPY";
    public double TakeProfitFactor { get; set; } = 0.01;
    public double StopLossFactor { get; set; } = 0.01;   
    public DateTime StartDate { get; set; } = DateTime.Now.AddYears(-1);
    public DateTime EndDate { get; set; } = DateTime.Now;
    public string Strategy { get; set; } = "Breakout";
    public string TimeFrame { get; set; } = "Day";  
}
