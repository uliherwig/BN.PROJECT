namespace BN.TRADER.AlpacaService
{
    public static class AlpacaAdapterExtensions
    {
        public static AlpacaBar ToAlpacaBar(this IBar bar)
        {
            return new AlpacaBar
            {
                C = bar.Close,
                H = bar.High,
                L = bar.Low,
                N = bar.TradeCount,
                O = bar.Open,
                T = bar.TimeUtc,
                V = bar.Volume,
                Vw = bar.Vwap
            };
        }

        public static AlpacaAsset ToAlpacaAsset(this IAsset asset)
        {
            return new AlpacaAsset
            {
                AssetId = asset.AssetId,          
                Name = asset.Name,
                Symbol = asset.Symbol,
            };
        }
    }
}