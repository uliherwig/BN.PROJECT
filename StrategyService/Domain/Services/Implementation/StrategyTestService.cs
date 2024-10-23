namespace BN.PROJECT.StrategyService;

public class StrategyTestService : IStrategyTestService
{
    private readonly ILogger<StrategyTestService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStrategyOperations _strategyOperations;

    private readonly ConcurrentDictionary<Guid, List<Position>> _positions = new();
    private readonly ConcurrentDictionary<Guid, StrategyProcessModel> _strategyProcesses = new();
    private readonly ConcurrentDictionary<Guid, BacktestSettings> _testSettings = new();

    public StrategyTestService(
        ILogger<StrategyTestService> logger,
        IServiceProvider serviceProvider,
        IStrategyOperations strategyOperations)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _strategyOperations = strategyOperations;
    }

    public Task StartTest(StrategyMessage message)
    {
        if (message.TestSettings == null)
        {
            return Task.CompletedTask;
        }

        var tpm = new StrategyProcessModel
        {
            StartDate = DateTime.MinValue,
            BreakoutTimeSpan = _strategyOperations.GetTimeSpanByBreakoutPeriod(message.TestSettings.BreakoutPeriod),
            CurrentHigh = 10000.0m,
            CurrentLow = 0.0m,
            PrevHigh = 10000.0m,
            PrevLow = 0.0m,
            LastTimeFrameVolume = 0,  // in use but senseless
            TimeFrameStart = DateTime.MinValue,
            PrevTimeFrameStart = DateTime.MinValue,
            TakeProfitPlus = 0.0m, // currently obsolete
            MarketCloseTime = new TimeSpan(19, 55, 0) // todo replace with method to get market close time get it from asset
        };
        _ = _strategyProcesses.TryAdd(message.TestId, tpm);
        _ = _testSettings.TryAdd(message.TestId, message.TestSettings);
        _ = _positions.TryAdd(message.TestId, []);
        return Task.CompletedTask;
    }


    public Task EvaluateQuote(Guid testId, Quote quote)
    {
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

            _logger.LogInformation($"#BT {tpm.TimeFrameStart} DIFF: {(tpm.PrevHigh - tpm.PrevLow).ToString("#.000")}     HIGH:{tpm.PrevHigh.ToString("000.000")} LOW:{tpm.PrevLow.ToString("000.000")}");
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
        if (_testSettings[testId].AllowOvernight == false && quoteStamp.TimeOfDay > tpm.MarketCloseTime)
        {
            var position = _positions[testId].FirstOrDefault(p => p.PriceClose == 0);
            if (position != null)
            {
                var closePrice = position.Side == Side.Buy ? quote.BidPrice : quote.AskPrice;
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
                var position = _strategyOperations.OpenPosition(_testSettings[testId], tpm, quote, Side.Buy);
                _positions[testId].Add(position);
            }

            // check breakout low
            if (quote.BidPrice < tpm.PrevLow)
            {
                var position = _strategyOperations.OpenPosition(_testSettings[testId], tpm, quote, Side.Sell);
                _positions[testId].Add(position);
            }
        }
        else
        {
            if (openPosition.Side == Side.Buy)
            {
                if (quote.AskPrice > openPosition.TakeProfit)
                {
                    if (_testSettings[testId].TrailingStop > 0m)
                    {
                        var tp = quote.AskPrice + (quote.AskPrice * _testSettings[testId].TakeProfitPercent / 100);
                        var sl = quote.BidPrice - (quote.BidPrice * _testSettings[testId].TrailingStop / 100);
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

            if (openPosition.Side == Side.Sell)
            {
                if (quote.BidPrice < openPosition.TakeProfit)
                {
                    if (_testSettings[testId].TrailingStop > 0m)
                    {
                        var tp = quote.BidPrice - (quote.BidPrice * _testSettings[testId].TakeProfitPercent / 100);
                        var sl = quote.AskPrice + (quote.AskPrice * _testSettings[testId].TrailingStop / 100);
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
        return Task.CompletedTask;
    }
    public async Task StopTest(StrategyMessage message)
    {
        _logger.LogInformation("Backtest stopped");
        var testId = message.TestId;

        var pos = _positions[testId].ToList();

        var overNight = pos.Where(p => p.StampOpened.Day != p.StampClosed.Day).ToList();
        var profits = pos.Where(p => p.ProfitLoss > 0).ToList();
        var losses = pos.Where(p => p.ProfitLoss < 0).ToList();

        var buys = pos.Where(p => p.Side == Side.Buy).ToList();
        var sells = pos.Where(p => p.Side == Side.Sell).ToList();


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
        _testSettings.TryRemove(testId, out _);
        _positions.TryRemove(testId, out _);
        _logger.LogInformation($"#BT {testId} Test stopped");

    }
}
