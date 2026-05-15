namespace BN.PROJECT.StrategyService;

public static class StrategyOperations
{
    public static DateTime GetStartOfTimeSpan(DateTime dateTime, TimeSpan timeSpan)
    {
        long ticksSinceMidnight = dateTime.TimeOfDay.Ticks / timeSpan.Ticks;
        TimeSpan startOfTimeSpan = TimeSpan.FromTicks(ticksSinceMidnight * timeSpan.Ticks);
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).Add(startOfTimeSpan);
    }

    public static TimeSpan GetTimeSpanByBreakoutPeriod(TimeFrameEnum breakoutPeriod)
    {
        return breakoutPeriod switch
        {
            TimeFrameEnum.Day => TimeSpan.FromDays(1),
            TimeFrameEnum.Hour => TimeSpan.FromHours(1),
            TimeFrameEnum.Minute => TimeSpan.FromMinutes(1),
            TimeFrameEnum.TenMinutes => TimeSpan.FromMinutes(10),
            _ => TimeSpan.FromDays(1)
        };
    }

    public static decimal CalculateSharpeRatio(List<PositionModel> positions, DateTime startDate, DateTime endDate)
    {
        var riskFreeRate = 0.01m; // Assuming risk-free rate is 1% per year, you can adjust this as needed

        if (positions.Count == 0)
            return 0;

        // Group positions by day and calculate daily returns
        var dailyReturns = positions
            .Where(p => p.StampOpened >= startDate && p.StampClosed <= endDate)
            .GroupBy(p => p.StampClosed.Date)
            .Select(g => g.Sum(p => p.ProfitLoss))
            .ToList();

        if (dailyReturns.Count == 0)
            return 0;

        decimal averageReturn = dailyReturns.Average();
        decimal standardDeviation = (decimal)Math.Sqrt(dailyReturns.Select(r => Math.Pow((double)(r - averageReturn), 2)).Average());

        // Assuming risk-free rate is 0 for simplicity
        return standardDeviation != 0 ? (averageReturn - riskFreeRate) / standardDeviation : 0;
    }


    public static List<BarSignalModel> DataFrameToBarSignalList(DataFrame df)
    {
        int n = (int)df.Rows.Count;
        var signals = new List<BarSignalModel>(n);

        var tsCol = df.Columns["DT"];
        var openCol = df.Columns["Open"];
        var highCol = df.Columns["High"];
        var lowCol = df.Columns["Low"];
        var closeCol = df.Columns["Close"];
        var signalCol = df.Columns["Signal"];


        for (int i = 0; i < n; i++)
        {
            var ts = tsCol[i] is DateTime dt ? dt : Convert.ToDateTime(tsCol[i]);
            var open = Convert.ToDecimal(openCol[i]);
            var high = Convert.ToDecimal(highCol[i]);
            var low = Convert.ToDecimal(lowCol[i]);
            var close = Convert.ToDecimal(closeCol[i]);
            var signal = Convert.ToInt32(signalCol[i]);

            signals.Add(new BarSignalModel(i + 1, ts, open, high, low, close, signal));
        }

        return signals;
    }

    public static List<PositionModel> RunBacktest(StrategySettingsModel strategySettings, List<BarSignalModel> data)
    {
        var positions = new List<PositionModel>();

        decimal equity = 10000m;
        var sl = strategySettings.StopLossPercent;
        var tp = strategySettings.TakeProfitPercent;
        var asset = strategySettings.Asset;
        var spreadPerTrade = strategySettings.SpreadPerTrade;
        var overnightFeeRate = strategySettings.OvernightFeeRate;
        var reverseTrade = strategySettings.ReverseTrade;
        var closePositionsEod = strategySettings.ClosePositionEod;

        Guid positionId = Guid.Empty;
        decimal totalProfit = 0m;

        int lastIndex = data.Count - 1;

        for (int i = 0; i < data.Count; i++)
        {
            var row = data[i];
            var currentTimestamp = row.Timestamp;
            var currentSignal = row.Signal;
            var currentPrice = row.Close;

            var openPosition = positions.FirstOrDefault(p => p.StampClosed == DateTime.MinValue);

            if (openPosition != null)
            {
                var posSideMultiplier = openPosition.Side == SideEnum.Buy ? 1 : -1;
                var currentReturn = (currentPrice - openPosition.PriceOpen) * posSideMultiplier / openPosition.PriceOpen;

                // calculate overnight holds
                var entryDate = openPosition.StampOpened.Date;
                var currentDate = currentTimestamp.Date;
                var overNightHolds = (currentDate - entryDate).Days;

                var totalFee = 2 * spreadPerTrade * openPosition.Quantity;
                var overnightFee = overNightHolds * overnightFeeRate * openPosition.PriceOpen * openPosition.Quantity;
                var profit = (currentPrice - openPosition.PriceOpen) * posSideMultiplier * openPosition.Quantity - totalFee - overnightFee;

                // check take profit and stop loss
                if (currentReturn >= tp || currentReturn <= -sl)
                {
                    openPosition.ClosePosition(
                        currentTimestamp,
                        currentPrice,
                        currentReturn >= tp ? "TP" : "SL",
                        profit
                    );
                    totalProfit += profit;
                    equity += profit;
                }

                // check reverse trade
                if (reverseTrade && ((openPosition.Side == SideEnum.Buy && currentSignal == -1) || (openPosition.Side == SideEnum.Sell && currentSignal == 1)))
                {
                    openPosition.ClosePosition(
                        currentTimestamp,
                        currentPrice,
                        "REV",
                        profit
                    );
                    totalProfit += profit;
                    equity += profit;
                }

                // check end of day close
                if (closePositionsEod && currentTimestamp.Date > openPosition.StampOpened.Date)
                {
                    var eodPrice = data[i-1].Close;
                    var eodStamp = data[i-1].Timestamp;

                    var eodProfit = (eodPrice - openPosition.PriceOpen) * posSideMultiplier * openPosition.Quantity - totalFee - overnightFee;

                    openPosition.ClosePosition(
                                eodStamp,
                                eodPrice,
                                "EOD",
                                eodProfit
                            );
                    totalProfit += eodProfit;
                    equity += eodProfit;
                }
            }

            // check to open new position
            if (openPosition == null && (currentSignal == 1 || currentSignal == -1))
            {
                var side = currentSignal == 1 ? SideEnum.Buy : SideEnum.Sell;
                var quantity = strategySettings.Quantity;
                var takeProfitPrice = side == SideEnum.Buy ?
                    currentPrice * (1 + tp) :
                    currentPrice * (1 - tp);
                var stopLossPrice = side == SideEnum.Buy ?
                    currentPrice * (1 - sl) :
                    currentPrice * (1 + sl);
                var newPosition = PositionExtensions.CreatePosition(
                    strategySettings.Id,
                    asset,
                    quantity,
                    side,
                    currentPrice,
                    stopLossPrice,
                    takeProfitPrice,
                    currentTimestamp,
                    spreadPerTrade,
                    overnightFeeRate,
                    strategySettings.StrategyParams
                );
                positions.Add(newPosition);
            }

            // close last open position at the end of the backtest
            if (i == lastIndex && openPosition != null)
            {
                var posSideMultiplier = openPosition.Side == SideEnum.Buy ? 1 : -1;
                var currentReturn = (currentPrice - openPosition.PriceOpen) * posSideMultiplier / openPosition.PriceOpen;
                var entryDate = openPosition.StampOpened.Date;
                var currentDate = currentTimestamp.Date;
                var overNightHolds = (currentDate - entryDate).Days;
                var totalFee = 2 * spreadPerTrade * openPosition.Quantity;
                var overnightFee = overNightHolds * overnightFeeRate * openPosition.PriceOpen * openPosition.Quantity;
                var profit = (currentPrice - openPosition.PriceOpen) * posSideMultiplier * openPosition.Quantity - totalFee - overnightFee;
                openPosition.ClosePosition(
                    currentTimestamp,
                    currentPrice,
                    "END",
                    profit
                );
                totalProfit += profit;
                equity += profit;
            }
        }

        strategySettings.StampEnd = DateTime.UtcNow.ToUniversalTime();
        return positions;
    }
}

