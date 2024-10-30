namespace BN.PROJECT.AlpacaService;

public class AlpacaAsset
{
    [Key]
    public Guid AssetId { get; set; }
    public required string Symbol { get; set; }
    public required string Name { get; set; }
    public TimeSpan MarketCloseTime { get; set; } = new TimeSpan(19, 55, 0);
}