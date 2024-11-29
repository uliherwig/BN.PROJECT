using NuGet.Configuration;

namespace BN.PROJECT.StrategyService;

public class SMAStrategy : IStrategyService
{
    private readonly ILogger<SMAStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStrategyOperations _strategyOperations;

    private readonly ConcurrentDictionary<Guid, List<Position>> _positions = new();
    private readonly ConcurrentDictionary<Guid, SMAProcessModel> _strategyProcesses = new();

    public SMAStrategy(
        ILogger<SMAStrategy> logger,
        IServiceProvider serviceProvider,
        IStrategyOperations strategyOperations)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _strategyOperations = strategyOperations;
    }

    public Task StartTest(StrategyMessage message)
    {
        if (message == null || message.Settings == null)
        {
            return Task.CompletedTask;
        }
        var strategySettings = message.Settings;
        var smaStettings = JsonConvert.DeserializeObject<SMAModel>(strategySettings.StrategyParams);
        if (smaStettings == null)
        {
            return Task.CompletedTask;
        }

        var tpm = new SMAProcessModel
        {
            StrategyId = message.StrategyId,
            StartDate = DateTime.MinValue,
            Asset = strategySettings.Asset,
            Quantity = strategySettings.Quantity,
            IsIncreasing = false,
            AllowOvernight = strategySettings.AllowOvernight,
            TakeProfitPercent = strategySettings.TakeProfitPercent,
            StopLossPercent = strategySettings.StopLossPercent,
            TrailingStop = strategySettings.TrailingStop,
            ShortPeriod = smaStettings.ShortPeriod,
            LongPeriod = smaStettings.LongPeriod,
            MarketCloseTime = new TimeSpan(19, 55, 0)
        };

        _ = _strategyProcesses.TryAdd(message.StrategyId, tpm);
        _ = _positions.TryAdd(message.StrategyId, []);
        return Task.CompletedTask;
    }


    public Task EvaluateQuote(Guid testId, Quote quote)
    {
        if (!_strategyProcesses.ContainsKey(testId) || (quote == null || quote.AskPrice == 0 || quote.BidPrice == 0))
        {
            return Task.CompletedTask;
        }

        var tpm = _strategyProcesses[testId];
        var quoteStamp = quote.TimestampUtc;

        tpm.LastQuotes.Add(quote);

        if (tpm.LastQuotes.Count <= tpm.LongPeriod)
        {
            return Task.CompletedTask;
        }
        tpm.LastQuotes.RemoveAt(0);
        var shortSma = tpm.LastQuotes.Average(q => q.AskPrice);
        var longSma = tpm.LastQuotes.Skip(tpm.LongPeriod - tpm.ShortPeriod).Take(tpm.ShortPeriod).Average(q => q.AskPrice);

        var lastIncrease = tpm.IsIncreasing;

        var trigger = SignalEnum.None;
        tpm.IsIncreasing = shortSma > longSma;

        if (lastIncrease != tpm.IsIncreasing)
        {
            trigger = tpm.IsIncreasing ? SignalEnum.Buy : SignalEnum.Sell;
        }

        var openPosition = _positions[testId].Where(p => p.PriceClose == 0).FirstOrDefault();
        if (openPosition == null)
        {
            var smaParams = JsonConvert.SerializeObject(new
            {
                ShortPeriod = tpm.ShortPeriod,
                LongPeriod = tpm.LongPeriod,
            });

            // check breakout high
            if (trigger == SignalEnum.Buy)
            {
                var position = PositionExtensions.CreatePosition(
                              tpm.StrategyId,
                              tpm.Asset,
                              tpm.Quantity,
                              SideEnum.Buy,
                              quote.AskPrice,
                              quote.BidPrice - (quote.BidPrice * tpm.TakeProfitPercent / 100),
                              quote.BidPrice + (quote.BidPrice * tpm.TakeProfitPercent / 100),
                              quote.TimestampUtc,
                              StrategyEnum.SimpleMovingAverage,
                              smaParams);
                _positions[testId].Add(position);
            }

            // check breakout low
            if (trigger == SignalEnum.Sell)
            {
                var position = PositionExtensions.CreatePosition(
                             tpm.StrategyId,
                             tpm.Asset,
                             tpm.Quantity,
                             SideEnum.Sell,
                             quote.BidPrice,
                             quote.AskPrice + (quote.AskPrice * tpm.TakeProfitPercent / 100),
                             quote.AskPrice - (quote.AskPrice * tpm.TakeProfitPercent / 100),
                             quote.TimestampUtc,
                             StrategyEnum.SimpleMovingAverage,
                             smaParams);
                _positions[testId].Add(position);
            }
        }
        else
        {
            // check if we need to close position at EoD 
            if (tpm.AllowOvernight == false && quoteStamp.TimeOfDay > tpm.MarketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
                return Task.CompletedTask;
            }

            if (openPosition.Side == SideEnum.Buy)
            {
                if (quote.BidPrice > openPosition.TakeProfit)
                {
                    if (tpm.TrailingStop > 0m)
                    {
                        var tp = quote.BidPrice + (quote.BidPrice * tpm.TakeProfitPercent / 100);
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

        var profit = pos.Sum(p => p.ProfitLoss);

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
     strategy == StrategyEnum.SimpleMovingAverage;
}
