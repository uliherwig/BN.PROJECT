namespace BN.PROJECT.Core;

public class StrategySettingsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StampStart { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public DateTime StampEnd { get; set; } = new DateTime(1, 1, 1, 0, 0, 0).ToUniversalTime();
    public Guid UserId { get; set; }
    public StrategyEnum StrategyType { get; set; }
    public string Broker { get; set; }
    public string Name { get; set; }
    public string Asset { get; set; }
    public int Quantity { get; set; }
    public decimal TakeProfitPercent { get; set; }
    public decimal StopLossPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TrailingStop { get; set; } = 0m;
    public bool ClosePositionEod { get; set; } = true;
    public bool Bookmarked { get; set; } = false;
    // TODO check if Optimized is needed
    public bool Optimized { get; set; } = false;
    public string StrategyParams { get; set; } = string.Empty;

    public decimal SpreadPerTrade { get; set; } = 0.005m;
    public decimal OvernightFeeRate { get; set; } = 0.00005m;
    public bool ReverseTrade { get; set; } = false;
}