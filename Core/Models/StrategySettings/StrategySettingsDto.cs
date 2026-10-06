namespace BN.PROJECT.Core;

public class StrategySettingsDTO
{
    public string Broker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public StrategyEnum StrategyType { get; set; } = StrategyEnum.NONE;
    public string Asset { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public TimeFrameEnum TimeFrame { get; set; } = TimeFrameEnum.Day;
    public decimal TakeProfitPercent { get; set; } = 0.0m;
    public decimal StopLossPercent { get; set; } = 0.0m;
    public DateTime StartDate { get; set; } = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public decimal TrailingStop { get; set; } = 0.0m;
    public bool ClosePositionEod { get; set; } = true;
    public decimal PositionTimeout { get; set; } = 60.0m;
    public decimal SpreadPerTrade { get; set; } = 0.005m;
    public decimal OvernightFeeRate { get; set; } = 0.00005m;
    public bool ReverseTrade { get; set; } = false;
}