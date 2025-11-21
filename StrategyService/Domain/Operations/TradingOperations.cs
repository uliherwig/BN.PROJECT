namespace BN.PROJECT.StrategyService;

public static class TradingOperations
{
    public static void UpdateOrCloseOpenPosition(ref PositionModel openPosition, Quote quote, decimal trailingStop, decimal takeProfitPercent)
    {
        if (openPosition.Side == SideEnum.Buy)
        {
            if (quote.BidPrice > openPosition.TakeProfit)
            {
                if (trailingStop > 0m)
                {
                    var tp = quote.BidPrice + (quote.BidPrice * takeProfitPercent / 100);
                    var sl = quote.BidPrice - (quote.BidPrice * trailingStop / 100);
                    openPosition.UpdateTakeProfitAndStopLoss(tp, sl);
                }
                else
                {
                    openPosition.ClosePosition(quote.TimestampUtc, quote.BidPrice, "TP");
                }
            }

            if (quote.BidPrice < openPosition.StopLoss)
            {
                openPosition.ClosePosition(quote.TimestampUtc, quote.BidPrice, "SL");
            }
        }

        if (openPosition.Side == SideEnum.Sell)
        {
            if (quote.AskPrice < openPosition.TakeProfit)
            {
                if (trailingStop > 0m)
                {
                    var tp = quote.AskPrice - (quote.AskPrice * takeProfitPercent / 100);
                    var sl = quote.AskPrice + (quote.AskPrice * trailingStop / 100);
                    openPosition.UpdateTakeProfitAndStopLoss(tp, sl);
                }
                else
                {
                    openPosition.ClosePosition(quote.TimestampUtc, quote.AskPrice, "TP");
                }
            }

            if (quote.AskPrice > openPosition.StopLoss)
            {
                openPosition.ClosePosition(quote.TimestampUtc, quote.AskPrice, "SL");
            }
        }
    }

    public static OrderMessage CreateOrderMessage(Guid strategyId, Guid userId, PositionModel position)
    {
        return new OrderMessage
        {
            MessageType = MessageTypeEnum.Order,
            StrategyId = strategyId,
            UserId = userId,
            Position = position
        };
    }

}
