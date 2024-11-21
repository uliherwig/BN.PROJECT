using BN.PROJECT.Core;
using System.Diagnostics;

namespace BN.PROJECT.StrategyService;

public class BreakoutStrategy : IStrategyService
{
    private readonly ILogger<BreakoutStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStrategyOperations _strategyOperations;

    private readonly ConcurrentDictionary<Guid, List<Position>> _positions = new();
    private readonly ConcurrentDictionary<Guid, BreakoutProcessModel> _strategyProcesses = new();
    //private readonly ConcurrentDictionary<Guid, StrategySettingsModel> _testSettings = new();

    public BreakoutStrategy(
        ILogger<BreakoutStrategy> logger,
        IServiceProvider serviceProvider,
        IStrategyOperations strategyOperations)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _strategyOperations = strategyOperations;
    }

    public async Task StartTest(StrategyMessage message)
    {
        if (message == null)
        {
            return;
        }
        var strategySettings = message.Settings;
        if (strategySettings == null)
        {
            return;
        }
        var breakOutSettings = _strategyOperations.GetBreakoutModel(strategySettings);

        var tpm = new BreakoutProcessModel
        {
            StartDate = DateTime.MinValue,
            BreakoutTimeSpan = _strategyOperations.GetTimeSpanByBreakoutPeriod(breakOutSettings.BreakoutPeriod),
            CurrentHigh = 10000.0m,
            CurrentLow = 0.0m,
            PrevHigh = 10000.0m,
            PrevLow = 0.0m,
            LastTimeFrameVolume = 0,  // in use but senseless
            TimeFrameStart = DateTime.MinValue,
            PrevTimeFrameStart = DateTime.MinValue,
            TakeProfitPlus = 0.0m, // currently obsolete
            MarketCloseTime = new TimeSpan(19, 55, 0), // todo replace with method to get market close time get it from asset
            AllowOvernight = strategySettings.AllowOvernight,
            Asset = strategySettings.Asset,
            Id = message.StrategyId,
            TakeProfitPercent = strategySettings.TakeProfitPercent,
            StopLossPercent = strategySettings.StopLossPercent,
            TrailingStop = strategySettings.TrailingStop,
        };
        _ = _strategyProcesses.TryAdd(message.StrategyId, tpm);
        _ = _positions.TryAdd(message.StrategyId, []);
        return;
    }


    public Task EvaluateQuote(Guid testId, Quote quote)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!_strategyProcesses.ContainsKey(testId))
        {
            return Task.CompletedTask;
        }

        if (quote == null || quote.AskPrice == 0 || quote.BidPrice == 0)
        {
            return Task.CompletedTask;
        }


        var tpm = _strategyProcesses[testId];
        var quoteStamp = quote.TimestampUtc;
        if (tpm.StartDate == DateTime.MinValue)
        {

            tpm.StartDate = quoteStamp;
            tpm.TimeFrameStart = _strategyOperations.GetStartOfTimeSpan(quoteStamp, tpm.BreakoutTimeSpan);
            tpm.PrevTimeFrameStart = tpm.TimeFrameStart;
            tpm.CurrentHigh = quote.AskPrice;
            tpm.CurrentLow = quote.BidPrice;
        }
        tpm.TimeFrameStart = _strategyOperations.GetStartOfTimeSpan(quoteStamp, tpm.BreakoutTimeSpan);

        if (tpm.TimeFrameStart > tpm.PrevTimeFrameStart)
        {
            tpm.LastTimeFrameVolume++;
            tpm.PrevTimeFrameStart = tpm.TimeFrameStart;

            if (tpm.CurrentHigh > tpm.PrevHigh || tpm.CurrentLow < tpm.PrevLow || tpm.LastTimeFrameVolume < 2)
            {
                tpm.PrevHigh = tpm.CurrentHigh;
                tpm.PrevHighStamp = tpm.CurrentHighStamp;
                tpm.PrevLow = tpm.CurrentLow;
                tpm.PrevLowStamp = tpm.CurrentLowStamp;
            }

            tpm.TakeProfitPlus = (tpm.PrevHigh - tpm.PrevLow) / 2;
            tpm.CurrentHigh = quote.AskPrice;
            tpm.CurrentHighStamp = quote.TimestampUtc;
            tpm.CurrentLow = quote.BidPrice;
            tpm.CurrentLowStamp = quote.TimestampUtc;
        }

        if (quoteStamp > tpm.TimeFrameStart && quoteStamp < tpm.TimeFrameStart.Add(tpm.BreakoutTimeSpan))
        {
            if (quote.AskPrice > tpm.CurrentHigh)
            {
                tpm.CurrentHigh = quote.AskPrice;
                tpm.CurrentHighStamp = quote.TimestampUtc;
            }
            if (quote.BidPrice < tpm.CurrentLow)
            {
                tpm.CurrentLow = quote.BidPrice;
                tpm.CurrentLowStamp = quote.TimestampUtc;
            }
        }

        // not ready to start trading
        if (tpm.PrevLow == 0)
        {
            return Task.CompletedTask;
        }


        // check if we need to close position at EoD 
        if (tpm.AllowOvernight == false && quoteStamp.TimeOfDay > tpm.MarketCloseTime)
        {
            var position = _positions[testId].FirstOrDefault(p => p.PriceClose == 0);
            if (position != null)
            {
                var closePrice = position.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                position.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
            }
            return Task.CompletedTask;
        }


        var openPosition = _positions[testId].Where(p => p.PriceClose == 0).FirstOrDefault();
        if (openPosition == null)
        {

            // check breakout high
            if (quote.AskPrice > tpm.PrevHigh)
            {
                var position = _strategyOperations.OpenPosition( tpm, quote, SideEnum.Buy);
                _positions[testId].Add(position);
            }

            // check breakout low
            if (quote.BidPrice < tpm.PrevLow)
            {
                var position = _strategyOperations.OpenPosition( tpm, quote, SideEnum.Sell);
                _positions[testId].Add(position);
            }
        }
        else
        {
            if (openPosition.Side == SideEnum.Buy)
            {
                if (quote.AskPrice > openPosition.TakeProfit)
                {
                    if (tpm.TrailingStop > 0m)
                    {
                        var tp = quote.AskPrice + (quote.AskPrice * tpm.TakeProfitPercent / 100);
                        var sl = quote.BidPrice - (quote.BidPrice * tpm.TrailingStop / 100);
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
                if (quote.BidPrice < openPosition.TakeProfit)
                {
                    if (tpm.TrailingStop > 0m)
                    {
                        var tp = quote.BidPrice - (quote.BidPrice * tpm.TakeProfitPercent / 100);
                        var sl = quote.AskPrice + (quote.AskPrice * tpm.TrailingStop / 100);
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
        stopwatch.Stop();
        _logger.LogInformation($"Breakout EvaluateQuote done in {stopwatch.Elapsed.TotalNanoseconds} ns");
        return Task.CompletedTask;
    }
    public async Task StopTest(StrategyMessage message)
    {
        _logger.LogInformation("Backtest stopped");
        var testId = message.StrategyId;

        var pos = _positions[testId].ToList();

        var overNight = pos.Where(p => p.StampOpened.Day != p.StampClosed.Day).ToList();
        var profits = pos.Where(p => p.ProfitLoss > 0).ToList();
        var losses = pos.Where(p => p.ProfitLoss < 0).ToList();

        var buys = pos.Where(p => p.Side == SideEnum.Buy).ToList();
        var sells = pos.Where(p => p.Side == SideEnum.Sell).ToList();


        _logger.LogInformation($"#BT OVERNIGHT POSITIONS: {overNight.Count}  Profit: {overNight.Sum(p => p.ProfitLoss)}");
        _logger.LogInformation($"#BT PROFIT POSITIONS: {profits.Count}  Profit: {profits.Sum(p => p.ProfitLoss)}");
        _logger.LogInformation($"#BT LOSS POSITIONS: {losses.Count}  Loss: {losses.Sum(p => p.ProfitLoss)}");

        _logger.LogInformation($"#BT BUY POSITIONS: {profits.Count}  Profit: {profits.Sum(p => p.ProfitLoss)}");
        _logger.LogInformation($"#BT SELL POSITIONS: {losses.Count}  Profit: {losses.Sum(p => p.ProfitLoss)}");

        var profit = pos.Sum(p => p.ProfitLoss);
        _logger.LogInformation($"#BT POSITIONS: {pos.Count}  Profit: {profit}");
        using (var scope = _serviceProvider.CreateScope())
        {
            var strategyRepository = scope.ServiceProvider.GetRequiredService<IStrategyRepository>();
            await strategyRepository.AddPositionsAsync(pos);
        }



        _strategyProcesses.TryRemove(testId, out _);
        _positions.TryRemove(testId, out _);
        _logger.LogInformation($"#BT {testId} Test stopped");

    }

    public bool CanHandle(StrategyEnum strategy) =>
     strategy == StrategyEnum.Breakout;
}
