namespace BN.PROJECT.AlpacaService
{
    public static class AlpacaAdapterExtensions
    {

        public static BrokerAccount ToBrokerAccount(this IAccount account, AccountStatusEnum accountStatus, Guid userId)
        {
            return new BrokerAccount
            {
                AccountStatus = accountStatus,
                UserId = userId,
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccruedFees = account.AccruedFees,
                BuyingPower = account.BuyingPower,
                CreatedAtUtc = account.CreatedAtUtc

            };
        }
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
                Symbol = asset.Symbol
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

        public static AlpacaQuote ToAlpacaQuote(this IQuote quote)
        {
            /*
                {
                "ap": 760.37,  // Ask: 760.37 $ (Käufer zahlen diesen Preis)
                "as": 80,      // 80 Aktien zum Ask-Preis verfügbar
                "ax": "V",     // Ask kommt von NYSE
                "bp": 760.25,  // Bid: 760.25 $ (Verkäufer erhalten diesen Preis)
                "bs": 80,      // 80 Aktien zum Bid-Preis nachgefragt
                "bx": "V",     // Bid kommt von NYSE
                "c": ["R"],    // Regular Sale (normaler Handel)
                "t": "2026-08-04T12:29:32.961151393Z",  // Zeitstempel (UTC)
                "z": "B"       // Trade wurde auf NASDAQ gemeldet
                }
             */




            return new AlpacaQuote
            {
                Symbol = quote.Symbol,
                AskPrice = quote.AskPrice,
                AskSize = quote.AskSize,
                BidPrice = quote.BidPrice,
                BidSize = quote.BidSize,
                TimestampUtc = quote.TimestampUtc,
                BidExchange = quote.BidExchange,
                AskExchange = quote.AskExchange,
                Tape = quote.Tape
            };  
        }

        public static AlpacaTrade ToAlpacaTrade(this ITrade trade)
        {
            /*
                {
                "p": 760.30,  // Ausführungspreis
                "s": 100,     // Ausgeführtes Volumen (Anzahl Aktien)
                "x": "V",     // Börse (z. B. "V" für NYSE, "Q" für NASDAQ)
                "t": "2026-08-04T12:29:32.961151393Z",  // Zeitstempel (UTC)
                "c": ["R"],   // Handelsbedingungen (z. B. ["R"] für Regular Sale)
                "i": 12345,   // Eindeutige Trade-ID
                "z": "A"      // Tape (z. B. "A", "B", "C")
                }
             */

            return new AlpacaTrade
            {
                Symbol = trade.Symbol,
                Price = trade.Price,
                Size = trade.Size,
                TimestampUtc = trade.TimestampUtc,
                Exchange = trade.Exchange,
                Conditions = trade.Conditions.ToArray(),
                TradeId = (long)trade.TradeId,
                Tape = trade.Tape
            };
        }

    }
}