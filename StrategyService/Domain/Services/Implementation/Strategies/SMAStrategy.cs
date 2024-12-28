namespace BN.PROJECT.StrategyService;

public class SmaStrategy : IStrategyService
{
    private readonly ILogger<SmaStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStrategyOperations _strategyOperations;

    public readonly ConcurrentDictionary<Guid, List<Position>> _positions = new();
    private readonly ConcurrentDictionary<Guid, SmaProcessModel> _strategyProcesses = new();
    private readonly ConcurrentDictionary<Guid, List<SmaTick>> _ticks = new();

    public SmaStrategy(
        ILogger<SmaStrategy> logger,
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
        var smaSettings = JsonConvert.DeserializeObject<SmaModel>(strategySettings.StrategyParams);
        if (smaSettings == null)
        {
            return Task.CompletedTask;
        }

        var tpm = new SmaProcessModel
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
            ShortPeriod = smaSettings.ShortPeriod,
            LongPeriod = smaSettings.LongPeriod,
            StopLossType = smaSettings.StopLossType,
            IntersectionThreshold = smaSettings.IntersectionThreshold,
            MarketCloseTime = new TimeSpan(19, 55, 0),

        };

        _ = _strategyProcesses.TryAdd(message.StrategyId, tpm);
        _ = _positions.TryAdd(message.StrategyId, []);
        _ = _ticks.TryAdd(message.StrategyId, []);

        return Task.CompletedTask;
    }


    public Task EvaluateQuote(Guid strategyId, Quote quote)
    {
        if (!_strategyProcesses.ContainsKey(strategyId) || (quote == null || quote.AskPrice == 0 || quote.BidPrice == 0))
        {
            return Task.CompletedTask;
        }

        var tpm = _strategyProcesses[strategyId];
        var quoteStamp = quote.TimestampUtc;
        var currentSmaTick = new SmaTick
        {
            StrategyId = strategyId,
            AskPrice = quote.AskPrice,
            BidPrice = quote.BidPrice,
            Asset = tpm.Asset,
            TimestampUtc = quote.TimestampUtc,
            ShortSma = 0,
            LongSma = 0
        };

        tpm.LastQuotes.Add(quote);

        if (tpm.LastQuotes.Count <= tpm.LongPeriod)
        {
            return Task.CompletedTask;
        }
        tpm.LastQuotes.RemoveAt(0);

        var shortSma = tpm.LastQuotes.Average(q => q.AskPrice);
        var longSma = tpm.LastQuotes.Skip(tpm.LongPeriod - tpm.ShortPeriod).Take(tpm.ShortPeriod).Average(q => q.AskPrice);

        var lastIncrease = tpm.IsIncreasing;

        var trigger = SideEnum.None;
        var closeTrigger = false;
        tpm.IsIncreasing = shortSma > longSma;

        if (lastIncrease != tpm.IsIncreasing && tpm.LastSmas.Count > 2)
        {
            closeTrigger = true;
            var lastDiff = tpm.LastSmas.Last().ShortSma - tpm.LastSmas.Last().LongSma;
            var diff = shortSma - longSma;
            if (Math.Abs(diff - lastDiff) > tpm.IntersectionThreshold)
            {
                trigger = tpm.IsIncreasing ? SideEnum.Buy : SideEnum.Sell;
            }
        }

        if (tpm.LastSmas.Count > 10)
        {
            tpm.LastSmas.RemoveAt(0);
        }

        currentSmaTick.ShortSma = shortSma;
        currentSmaTick.LongSma = longSma;
        _ticks[strategyId].Add(currentSmaTick);

        tpm.LastSmas.Add(currentSmaTick);


        var openPosition = _positions[strategyId].Where(p => p.PriceClose == 0).FirstOrDefault();
        if (openPosition == null)
        {
            var smaParams = JsonConvert.SerializeObject(new
            {
                ShortPeriod = tpm.ShortPeriod,
                LongPeriod = tpm.LongPeriod,
                StopLossType = tpm.StopLossType,
                IntersectionThreshold = tpm.IntersectionThreshold
            });

            // check breakout high
            if (trigger == SideEnum.Buy)
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
                              StrategyEnum.SMA,
                              smaParams);
                _positions[strategyId].Add(position);
            }

            // check breakout low
            if (trigger == SideEnum.Sell)
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
                             StrategyEnum.SMA,
                             smaParams);
                _positions[strategyId].Add(position);
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

            if (tpm.StopLossType == StopLossTypeEnum.None)
            {
                _strategyOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, tpm.TrailingStop, tpm.TakeProfitPercent);
            }
            if (tpm.StopLossType == StopLossTypeEnum.SMANextCrossing && closeTrigger)
            {
                openPosition.ClosePosition(quote.TimestampUtc, quote.BidPrice, "MA");
            }
        }
        return Task.CompletedTask;
    }

    public async Task StopTest(StrategyMessage message)
    {
        if(message == null || !_strategyProcesses.ContainsKey(message.StrategyId))
        {
            return;
        }
        try
        {
            _logger.LogInformation("Backtest stopped");        

            var pos = _positions[message.StrategyId].ToList();
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

            // TODO remove asap
            var ticks = _ticks[message.StrategyId];
            var csvBuilder = new StringBuilder();

            // CSV-Header
            csvBuilder.AppendLine("TimestampUtc;AskPrice;ShortSma;LongSma");

            // CSV-Daten
            foreach (var tick in ticks)
            {
                csvBuilder.AppendLine($"{tick.TimestampUtc.ToShortTimeString()};{tick.AskPrice.ToString("F3")};{tick.BidPrice.ToString("F3")};{tick.ShortSma.ToString("F3")};{tick.LongSma.ToString("F3")};{(tick.ShortSma - tick.LongSma).ToString("F3")}");


            }

            var test = csvBuilder.ToString();

            _strategyProcesses.TryRemove(message.StrategyId, out _);
            _positions.TryRemove(message.StrategyId, out _);
            _ticks.TryRemove(message.StrategyId, out _);
            _logger.LogInformation($"#BT {message.StrategyId} Test stopped");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in StopTest");
        }
    }

    public List<Position>? GetPositions(Guid strategyId)
    {
        var positions = _positions.ContainsKey(strategyId) ? _positions[strategyId] : null;
        return positions;
    }

    public bool CanHandle(StrategyEnum strategy) =>
         strategy == StrategyEnum.SMA;

}
