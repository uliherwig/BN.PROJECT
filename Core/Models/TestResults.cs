namespace BN.PROJECT.Core;

public class TestResult
{
    public Guid Id { get; set; }
    public string Symbol { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BreakoutPeriod TimeFrame { get; set; }
    public int NumberOfPositions { get; set; }
    public int NumberOfBuyPositions { get; set; }
    public int NumberOfSellPositions { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal BuyProfitLoss { get; set; }
    public decimal SellProfitLoss { get; set; }

}
