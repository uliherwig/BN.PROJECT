namespace BN.PROJECT.DataService;

public class AlpacaAsset
{
    [Key]
    public Guid AssetId { get; set; }

    public string Symbol { get; set; }

    public string Name { get; set; }
}