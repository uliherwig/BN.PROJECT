namespace BN.TRADER.AlpacaService
{
    public static class AlpacaAdapterExtensions
    {
        public static AlpacaBar ToAlpacaBar(this IBar bar, string symbol)
        {
            return new AlpacaBar
            {
                Symbol = symbol,
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

        public static AlpacaOrder ToAlpacaOrder(this IOrder order)
        {
            return new AlpacaOrder
            {
                OrderId = order.OrderId,
                ClientOrderId = order.ClientOrderId,
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
                SubmittedAtUtc = order.SubmittedAtUtc,
                FilledAtUtc = order.FilledAtUtc,
                ExpiredAtUtc = order.ExpiredAtUtc,
                CancelledAtUtc = order.CancelledAtUtc,
                FailedAtUtc = order.FailedAtUtc,
                ReplacedAtUtc = order.ReplacedAtUtc,
                AssetId = order.AssetId,
                Symbol = order.Symbol,
                Notional = order.Notional,
                Quantity = order.Quantity,
                FilledQuantity = order.FilledQuantity,
                IntegerQuantity = order.IntegerQuantity,
                IntegerFilledQuantity = order.IntegerFilledQuantity,
                OrderType = order.OrderType,
                OrderClass = order.OrderClass,
                OrderSide = order.OrderSide,
                TimeInForce = order.TimeInForce,
                LimitPrice = order.LimitPrice,
                StopPrice = order.StopPrice,
                TrailOffsetInDollars = order.TrailOffsetInDollars,
                TrailOffsetInPercent = order.TrailOffsetInPercent,
                HighWaterMark = order.HighWaterMark,
                AverageFillPrice = order.AverageFillPrice,
                OrderStatus = order.OrderStatus,
                ReplacedByOrderId = order.ReplacedByOrderId,
                ReplacesOrderId = order.ReplacesOrderId
            };
        }

        public static AlpacaPosition ToAlpacaPosition(this IPosition position)
        {
            return new AlpacaPosition
            {
                AssetId = position.AssetId,
                Symbol = position.Symbol,
                Exchange = position.Exchange,
                AssetClass = position.AssetClass,
                AverageEntryPrice = position.AverageEntryPrice,
                Quantity = position.Quantity,
                IntegerQuantity = position.IntegerQuantity,
                AvailableQuantity = position.AvailableQuantity,
                IntegerAvailableQuantity = position.IntegerAvailableQuantity,
                Side = position.Side,
                MarketValue = position.MarketValue,
                CostBasis = position.CostBasis,
                UnrealizedProfitLoss = position.UnrealizedProfitLoss,
                UnrealizedProfitLossPercent = position.UnrealizedProfitLossPercent,
                IntradayUnrealizedProfitLoss = position.IntradayUnrealizedProfitLoss,
                IntradayUnrealizedProfitLossPercent = position.IntradayUnrealizedProfitLossPercent,
                AssetCurrentPrice = position.AssetCurrentPrice,
                AssetLastPrice = position.AssetLastPrice,
                AssetChangePercent = position.AssetChangePercent
            };
        }
    }
}