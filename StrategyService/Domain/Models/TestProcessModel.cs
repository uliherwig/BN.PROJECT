namespace BN.PROJECT.StrategyService;

public class TestProcessModel
{
    public List<Position> Positions { get; set; } = new List<Position>();
    public string Symbol { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Quote High { get; set; }
    public Quote Low { get; set; }
    public TimeSpan TimeFrame { get; set; }
    public decimal DifferenceHighLow { get; set; }
    public bool IsIncreasing { get; set; }





}
