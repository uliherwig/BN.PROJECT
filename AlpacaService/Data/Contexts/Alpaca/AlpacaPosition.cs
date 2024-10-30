namespace BN.PROJECT.AlpacaService;

public class AlpacaPosition
{
    [Key]
    public Guid AssetId { get; set; }
    public Guid UserId { get; set; }
    public string Symbol { get; set; }
    public Exchange Exchange { get; set; }
    public AssetClass AssetClass { get; set; }
    public decimal AverageEntryPrice { get; set; }
    public decimal Quantity { get; set; }
    public long IntegerQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public long IntegerAvailableQuantity { get; set; }
    public PositionSide Side { get; set; }
    public decimal? MarketValue { get; set; }
    public decimal CostBasis { get; set; }
    public decimal? UnrealizedProfitLoss { get; set; }
    public decimal? UnrealizedProfitLossPercent { get; set; }
    public decimal? IntradayUnrealizedProfitLoss { get; set; }
    public decimal? IntradayUnrealizedProfitLossPercent { get; set; }
    public decimal? AssetCurrentPrice { get; set; }
    public decimal? AssetLastPrice { get; set; }
    public decimal? AssetChangePercent { get; set; }
}