namespace BN.PROJECT.Core;

public static class PositionExtensions
{
    public static Position CreatePosition(
        Guid testId,
        string symbol,
        int quantity,
        SideEnum side,
        decimal priceOpen,
        decimal stopLoss,
        decimal takeProfit,
        DateTime stampOpen)
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            TestId = testId,
            Symbol = symbol,
            Quantity = quantity,
            Side = side,
            PriceOpen = priceOpen,
            TakeProfit = takeProfit,
            StopLoss = stopLoss,
            StampOpened = stampOpen.ToUniversalTime(),
            CloseSignal = "",
        };

        return position;
    }

    public static Position CreatePosition(
      Guid testId,
      string symbol,
      int quantity,
      SideEnum side,
      decimal priceOpen,
      decimal stopLoss,
      decimal takeProfit,
      DateTime stampOpen,
      decimal prevLow,
      decimal prevHigh,
      DateTime prevLowStamp,
      DateTime prevHighStamp)
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            TestId = testId,
            Symbol = symbol,
            Quantity = quantity,
            Side = side,
            PriceOpen = priceOpen,
            TakeProfit = takeProfit,
            StopLoss = stopLoss,
            StampOpened = stampOpen.ToUniversalTime(),
            CloseSignal = "",
            PrevHigh = prevHigh,
            PrevLow = prevLow,
            PrevHighStamp = prevHighStamp.ToUniversalTime(),
            PrevLowStamp = prevLowStamp.ToUniversalTime()
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