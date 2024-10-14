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
        var tpm = new TestProcessModel
        {
            Symbol = testSettings.Symbol,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.UtcNow,
            TimeFrame = TimeSpan.FromDays(1),
            DifferenceHighLow = 0.0m,
            IsIncreasing = false,
            CurrentHigh = 10000.0m,
            CurrentLow = 0.0m,
            PrevHigh = 10000.0m,
            PrevLow = 0.0m,
            LastTimeFrameVolume = 0,
            TimeFrameStart = DateTime.MinValue,
            PrevTimeFrameStart = DateTime.MinValue,
            TakeProfitPlus = 0.0m,
            MarketCloseTime = new TimeSpan(19, 50, 0)
        };
        _testProcesses.TryAdd(testSettings.Id, tpm);
        _testSettings.TryAdd(testSettings.Id, testSettings);
        _positions.TryAdd(testSettings.Id, new List<Position>());
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
        var tpm = _testProcesses[testId];

        if (tpm.StartDate == DateTime.MinValue)
        {
            var firstQuote = quotes.FirstOrDefault(q => q.AskPrice > 0 && q.BidPrice > 0);
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
                    tpm.PrevLow = tpm.CurrentLow;
                }


                tpm.TakeProfitPlus = (tpm.PrevHigh - tpm.PrevLow) / 2;
                tpm.CurrentHigh = quote.AskPrice;
                tpm.CurrentLow = quote.BidPrice;

                _logger.LogInformation($"#BT {tpm.TimeFrameStart} DIFF: {(tpm.PrevHigh - tpm.PrevLow).ToString("#.000")}     HIGH:{tpm.PrevHigh.ToString("000.000")} LOW:{tpm.PrevLow.ToString("000.000")}");
            }

            if (currentStamp > tpm.TimeFrameStart && currentStamp < tpm.TimeFrameStart.Add(tpm.TimeFrame))
            {
                tpm.CurrentHigh = quote.AskPrice > tpm.CurrentHigh ? quote.AskPrice : tpm.CurrentHigh;
                tpm.CurrentLow = quote.BidPrice < tpm.CurrentLow ? quote.BidPrice : tpm.CurrentLow;
            }

            if (tpm.PrevLow == 0)
            {
                continue;
            }

            var openPositionsLong = _positions[testId].Where(p => p.Side == Side.Buy && p.PriceClose == 0).ToList();
            var openPositionsShort = _positions[testId].Where(p => p.Side == Side.Sell && p.PriceClose == 0).ToList();

            if (quote.AskPrice > tpm.PrevHigh)
            {
                if (openPositionsLong.Count == 0)
                {
                    var position = PositionExtensions.CreatePosition(testId, tpm.Symbol, 1, Side.Buy, quote.AskPrice, tpm.PrevLow, quote.AskPrice + tpm.TakeProfitPlus, quote.TimestampUtc);
                    _positions[testId].Add(position);
                }
            }

            // check breakout low if decrease
            if (quote.BidPrice < tpm.PrevLow)
            {
                if (openPositionsShort.Count == 0)
                {
                    var position = PositionExtensions.CreatePosition(testId, tpm.Symbol, 1, Side.Sell, quote.BidPrice, tpm.PrevHigh, quote.BidPrice - tpm.TakeProfitPlus, quote.TimestampUtc);
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

            //if (quote.TimestampUtc.TimeOfDay > tpm.MarketCloseTime)
            //{
            //    var openPositionsLongEoD = _positionManager.GetOpenPositionsBySide(Side.Buy);
            //    var openPositionsShortEoD = _positionManager.GetOpenPositionsBySide(Side.Sell);
            //    foreach (var position in openPositionsLongEoD)
            //    {
            //        _positionManager.ClosePosition(position.Id, quote.TimestampUtc, quote.BidPrice, "End of Day");
            //    }
            //    foreach (var position in openPositionsShortEoD)
            //    {
            //        _positionManager.ClosePosition(position.Id, quote.TimestampUtc, quote.AskPrice, "End of Day");
            //    }
            //}
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
