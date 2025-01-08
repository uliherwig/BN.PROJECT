namespace BN.PROJECT.Core;

public static class PositionExtensions
{
    public static Position CreatePosition(
        Guid strategyId,
        string symbol,
        int quantity,
        SideEnum side,
        decimal priceOpen,
        decimal stopLoss,
        decimal takeProfit,
        DateTime stampOpen,
        StrategyEnum strategyType,
        string StrategyParameter)
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            StrategyId = strategyId,
            Symbol = symbol,
            Quantity = quantity,
            Side = side,
            PriceOpen = priceOpen,
            PriceClose = 0,
            ProfitLoss = 0,
            StampClosed = DateTime.MinValue,
            TakeProfit = takeProfit,
            StopLoss = stopLoss,
            StampOpened = stampOpen.ToUniversalTime(),
            CloseSignal = "",
            StrategyType = strategyType,
            StrategyParams = StrategyParameter
        };

        return position;
    }

    public static bool UpdateTakeProfitAndStopLoss(this Position position, decimal newTakeProfit, decimal newStopLoss)
    {
        position.TakeProfit = newTakeProfit;
        position.StopLoss = newStopLoss;
        position.CloseSignal = "Update ";
        return true;
    }

    public static void ClosePosition(this Position position, DateTime stampClose, decimal priceClose, string closeSignal)
    {
        position.PriceClose = priceClose;
        position.StampClosed = stampClose.ToUniversalTime();
        position.CloseSignal = closeSignal;
        position.ProfitLoss = (priceClose - position.PriceOpen) * position.Quantity * (position.Side == SideEnum.Buy ? 1 : -1);
    }
}