namespace BN.PROJECT.AlpacaService;

public class AlpacaAsset
{
    [Key]
    public Guid AssetId { get; set; }

    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TimeSpan MarketCloseTime { get; set; } = new TimeSpan(19, 55, 0);
}