namespace BN.PROJECT.Core;

public class StrategySettingsModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public StrategyEnum StrategyType { get; set; }
    public string Broker { get; set; }
    public string Name { get; set; }
    public string Asset { get; set; }
    public int Quantity { get; set; }
    public decimal TakeProfitPercent { get; set; }
    public decimal StopLossPercent { get; set; }
    public DateTime StartDate { get; set; } = DateTime.MinValue.ToUniversalTime();
    public DateTime EndDate { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public decimal TrailingStop { get; set; } = 0m;
    public bool AllowOvernight { get; set; } = true;
    public bool Bookmarked { get; set; } = true;
    public DateTime TestStamp { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public string StrategyParams { get; set; } = string.Empty;
}