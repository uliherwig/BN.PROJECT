namespace BN.PROJECT.Core;

public static class PositionExtensions
{
    public static PositionModel CreatePosition(
        Guid strategyId,
        string symbol,
        int quantity,
        SideEnum side,
        decimal priceOpen,
        decimal stopLoss,
        decimal takeProfit,
        DateTime stampOpen,
        decimal spreadPerTrade,
        decimal overnightFeeRate,
        string StrategyParameter)
    {
        var position = new PositionModel
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
            SpreadPerTrade = spreadPerTrade,
            OvernightFeeRate = overnightFeeRate,

            StrategyParams = StrategyParameter
        };

        return position;
    }

    public static bool UpdateTakeProfitAndStopLoss(this PositionModel position, decimal newTakeProfit, decimal newStopLoss)
    {
        position.TakeProfit = newTakeProfit;
        position.StopLoss = newStopLoss;
        position.CloseSignal = "Update ";
        return true;
    }

    public static void ClosePosition(this PositionModel position, DateTime stampClose, decimal priceClose, string closeSignal, decimal profit)
    {
        position.PriceClose = priceClose;
        position.StampClosed = stampClose.ToUniversalTime();
        position.CloseSignal = closeSignal;
        position.ProfitLoss = profit;
    }

    public static PositionModel GetOpenPosition(List<PositionModel> positions) => positions.FirstOrDefault(p => p.StampClosed == DateTime.MinValue);

}