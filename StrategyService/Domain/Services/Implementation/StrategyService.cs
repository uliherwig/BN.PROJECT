namespace BN.PROJECT.StrategyService;

public class StrategyService : IStrategyService
{
    private readonly ILogger<StrategyService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private ConcurrentDictionary<Guid, List<Position>> _positions = new ConcurrentDictionary<Guid, List<Position>>();
    private ConcurrentDictionary<Guid, TestProcessModel> _testProcesses = new ConcurrentDictionary<Guid, TestProcessModel>();
    private ConcurrentDictionary<Guid, BacktestSettings> _testSettings = new ConcurrentDictionary<Guid, BacktestSettings>();

    public StrategyService(ILogger<StrategyService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartTest(BacktestSettings testSettings)
    {
        TimeSpan timeFrame = testSettings.BreakoutPeriod switch
        {
            BreakoutPeriod.Day => TimeSpan.FromDays(1),
            BreakoutPeriod.Hour => TimeSpan.FromHours(1),
            BreakoutPeriod.Minute => TimeSpan.FromMinutes(1),
            BreakoutPeriod.TenMinutes => TimeSpan.FromMinutes(10),
            _ => TimeSpan.FromDays(1)
        };
        var tpm = new TestProcessModel
        {
            Symbol = testSettings.Symbol,
            StartDate = DateTime.MinValue,
            EndDate = testSettings.EndDate.ToUniversalTime(),  // not in use
            TimeFrame = timeFrame,
            DifferenceHighLow = 0.0m,// not in use
            IsIncreasing = false, // not in use
            CurrentHigh = 10000.0m,
            CurrentLow = 0.0m,
            PrevHigh = 10000.0m,
            PrevLow = 0.0m,
            LastTimeFrameVolume = 0,  // in use but senseless
            TimeFrameStart = DateTime.MinValue,
            PrevTimeFrameStart = DateTime.MinValue,
            TakeProfitPlus = 0.0m, // todo replace with method to get take profit plus
            AllowOvernight = testSettings.AllowOvernight,
            MarketCloseTime = new TimeSpan(19, 50, 0) // todo replace with method to get market close time get it from asset
        };
        _testProcesses.TryAdd(testSettings.Id, tpm);
        _testSettings.TryAdd(testSettings.Id, testSettings);
        _ = _positions.TryAdd(testSettings.Id, new List<Position>());
        return Task.CompletedTask;
    }


    public Task EvaluateQuotes(StrategyMessage message)
    {
        var testId = message.TestId;
        if (!_testProcesses.ContainsKey(testId))
        {
            return Task.CompletedTask;
        }

        var quotes = message.Quotes;
        if (quotes == null)
        {
            return Task.CompletedTask;
        }

        var firstQuote = quotes.FirstOrDefault();
        if (firstQuote == null)
        {
            return Task.CompletedTask;
        }
        var lastQuote = quotes.LastOrDefault();

        var firstQuoteDate = firstQuote.TimestampUtc;
        var lastQuoteDate = lastQuote.TimestampUtc;


        var tpm = _testProcesses[testId];
        tpm.MarketCloseTime = lastQuoteDate.AddMinutes(-3).TimeOfDay;
        if (tpm.StartDate == DateTime.MinValue)
        {
            firstQuote = quotes.FirstOrDefault(q => q.AskPrice > 0 && q.BidPrice > 0);
            if (firstQuote != null)
            {
                tpm.StartDate = firstQuote.TimestampUtc;
                tpm.TimeFrameStart = new DateTime(tpm.StartDate.Year, tpm.StartDate.Month, tpm.StartDate.Day, 0, 0, 0);
                tpm.PrevTimeFrameStart = tpm.TimeFrameStart;
                tpm.CurrentHigh = firstQuote.AskPrice;
                tpm.CurrentLow = firstQuote.BidPrice;
            }
        }

        foreach (var quote in quotes.Where(q => q.AskPrice > 0 && q.BidPrice > 0))
        {
            var currentStamp = quote.TimestampUtc;
            tpm.TimeFrameStart = new DateTime(currentStamp.Year, currentStamp.Month, currentStamp.Day, 0, 0, 0);  // todo replace with method to get time frame stamp



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

            if (currentStamp > tpm.TimeFrameStart && currentStamp < tpm.TimeFrameStart.Add(tpm.TimeFrame))
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

            if (tpm.PrevLow == 0)
            {
                continue;
            }
            if (tpm.AllowOvernight && quote.TimestampUtc.TimeOfDay > tpm.MarketCloseTime)
            {
                var position = _positions[testId].FirstOrDefault(p => p.PriceClose == 0);
                if (position != null)
                {
                    var closePrice = position.Side == Side.Buy ? quote.BidPrice : quote.AskPrice;
                    position.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
                }
                continue;
            }

            var openPositionsLong = _positions[testId].Where(p => p.Side == Side.Buy && p.PriceClose == 0).ToList();
            var openPositionsShort = _positions[testId].Where(p => p.Side == Side.Sell && p.PriceClose == 0).ToList();
            var createPositionLock = openPositionsLong.Count > 0 || openPositionsShort.Count > 0;



            if (quote.AskPrice > tpm.PrevHigh && (quote.AskPrice - tpm.PrevHigh) < 0.2m)
            {
                if (!createPositionLock)
                {
                    var position = PositionExtensions.CreatePosition(testId,
                                    tpm.Symbol,
                                    1,
                                    Side.Buy,
                                    quote.AskPrice,
                                    tpm.PrevLow,
                                    quote.AskPrice + tpm.TakeProfitPlus,
                                    quote.TimestampUtc,
                                    tpm.PrevLow,
                                    tpm.PrevHigh,
                                    tpm.PrevLowStamp,
                                    tpm.PrevHighStamp);
                    _positions[testId].Add(position);
                }
            }

            // check breakout low if decrease
            if (quote.BidPrice < tpm.PrevLow && (tpm.PrevLow - quote.BidPrice) < 0.2m)
            {
                if (!createPositionLock)
                {
                    var position = PositionExtensions.CreatePosition(testId,
                                    tpm.Symbol,
                                    1,
                                    Side.Sell,
                                    quote.BidPrice,
                                    tpm.PrevHigh,
                                   quote.BidPrice - tpm.TakeProfitPlus,
                                    quote.TimestampUtc,
                                    tpm.PrevLow,
                                    tpm.PrevHigh,
                                    tpm.PrevLowStamp,
                                    tpm.PrevHighStamp);

                    _positions[testId].Add(position);
                }
            }

            foreach (var position in openPositionsLong)
            {
                var pos = _positions[testId].FirstOrDefault(p => p.Id == position.Id);
                if (pos == null)
                {
                    continue;
                }

                if (quote.AskPrice > position.TakeProfit)
                {
                    pos.UpdateTakeProfitAndStopLoss(quote.AskPrice + 1m, quote.BidPrice - 1m);
                }
                else if (quote.AskPrice < position.StopLoss)
                {
                    pos.ClosePosition(quote.TimestampUtc, quote.BidPrice, "Stop Loss");
                }
            }

            foreach (var position in openPositionsShort)
            {
                var pos = _positions[testId].FirstOrDefault(p => p.Id == position.Id);
                if (pos == null)
                {
                    continue;
                }
                if (quote.BidPrice < position.TakeProfit)
                {
                    pos.UpdateTakeProfitAndStopLoss(quote.BidPrice - 1m, quote.AskPrice + 1m);
                }
                else if (quote.BidPrice > position.StopLoss)
                {
                    pos.ClosePosition(quote.TimestampUtc, quote.AskPrice, "Stop Loss");
                }
            }

            // close all positions at end of day


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



        _testProcesses.TryRemove(testId, out _);
        _testSettings.TryRemove(testId, out _);
        _positions.TryRemove(testId, out _);

    }
}
