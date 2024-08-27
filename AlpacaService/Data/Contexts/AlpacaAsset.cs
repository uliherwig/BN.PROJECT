namespace BN.TRADER.AlpacaService
{
    public class AlpacaAsset
    {
        [Key]
        public Guid AssetId { get; set; }

        public string Symbol { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }
    }
}