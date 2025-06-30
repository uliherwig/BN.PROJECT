using AutoMapper.Execution;
using NuGet.Protocol;

namespace BN.PROJECT.StrategyService;

public class SmaStrategy : IStrategyService
{
    private readonly ILogger<SmaStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStrategyOperations _strategyOperations;
    private readonly IKafkaProducerService _kafkaProducer;

    public readonly ConcurrentDictionary<Guid, List<PositionModel>> _positions = new();
    private readonly ConcurrentDictionary<Guid, SmaProcessModel> _strategyProcesses = new();
    private readonly ConcurrentDictionary<Guid, List<SmaTick>> _ticks = new();
    private readonly ConcurrentDictionary<Guid, List<SideEnum>> _signals = new();

    private readonly string _notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);
    private readonly string _orderTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Order);



    public SmaStrategy(
        ILogger<SmaStrategy> logger,
        IServiceProvider serviceProvider,
        IStrategyOperations strategyOperations,
        IKafkaProducerService kafkaProducer)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _strategyOperations = strategyOperations;
        _kafkaProducer = kafkaProducer;
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
            IsBacktest = message.IsBacktest,
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
            MarketCloseTime = new TimeSpan(19, 55, 0),
        };

        _ = _strategyProcesses.TryAdd(message.StrategyId, tpm);
        _ = _positions.TryAdd(message.StrategyId, []);
        _ = _ticks.TryAdd(message.StrategyId, []);
        _ = _signals.TryAdd(message.StrategyId, new List<SideEnum>());

        _kafkaProducer.SendMessageAsync(_notificationTopic, message.ToJson());


        return Task.CompletedTask;
    }

    public Task EvaluateQuote(Guid strategyId, Guid userId, Quote quote)
    {
        // how parameters are handled 
        // check signal short > long SMA => 0,1
        // check close method 
        // 1. intersection - means close and open signal at the same time
        // 2. stop loss - means close position if price is below stop loss
        // 3. take profit - means close position if price is above take profit
        // sl and tp must then be set both
        // 4. EoD - means close position at the end of the day

        if (!_strategyProcesses.ContainsKey(strategyId) || (quote == null || quote.AskPrice == 0 || quote.BidPrice == 0))
        {
            return Task.CompletedTask;
        }

        var tpm = _strategyProcesses[strategyId];
        var smaParams = JsonConvert.SerializeObject(new
        {
            ShortPeriod = tpm.ShortPeriod,
            LongPeriod = tpm.LongPeriod,
            StopLossType = tpm.StopLossType
        });
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

        var longSma = tpm.LastQuotes.Average(q => q.AskPrice);
        var shortSma = tpm.LastQuotes.Skip(tpm.LongPeriod - tpm.ShortPeriod).Take(tpm.ShortPeriod).Average(q => q.AskPrice);

        var order = false;

        var signal = shortSma > longSma ? SideEnum.Buy : SideEnum.Sell;
        _signals[strategyId].Add(signal);
        if (_signals[strategyId].Count == 2)
        {
            order = _signals[strategyId].ElementAt(0) != _signals[strategyId].ElementAt(1);
            if (order)
            {
                _logger.LogInformation($"Signals : {_signals[strategyId].ElementAt(0)} {_signals[strategyId].ElementAt(1)}");
            }

            _signals[strategyId].RemoveAt(0);
        }



        _logger.LogInformation($"SMA Intersection detected for strategy {strategyId} at {quoteStamp}; signal = {_signals[strategyId].ElementAt(0)}");


        currentSmaTick.ShortSma = shortSma;
        currentSmaTick.LongSma = longSma;
        _ticks[strategyId].Add(currentSmaTick);

        tpm.LastSmas.Add(currentSmaTick);

        var openPosition = _positions[strategyId].Where(p => p.PriceClose == 0).FirstOrDefault();


        // check first if we have to handle a openPosition
        if (openPosition != null)
        {
            // check if we need to close position at EoD
            if (tpm.AllowOvernight == false && quoteStamp.TimeOfDay > tpm.MarketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
                return Task.CompletedTask;
            }

            // handle normal SL / TP operation
            if (tpm.StopLossType == StopLossTypeEnum.None)
            {
                _strategyOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, tpm.TrailingStop, tpm.TakeProfitPercent);
            }

            if (tpm.StopLossType == StopLossTypeEnum.SMAIntersection && order)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "MA");

            }
            var test = _positions[strategyId].Where(p => p.PriceClose == 0).FirstOrDefault();
            if (openPosition.PriceClose > 0 && !tpm.IsBacktest)
            {
                var message = _strategyOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                _kafkaProducer.SendMessageAsync("order", message);
            }
        }
        if (_positions[strategyId].Where(p => p.PriceClose == 0).FirstOrDefault() == null)
        {
            if (order)
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
                if (_signals[strategyId].ElementAt(0) == SideEnum.Sell)
                {
                    position.PriceOpen = quote.BidPrice;
                    position.Side = SideEnum.Sell;
                    position.TakeProfit = quote.AskPrice + (quote.AskPrice * tpm.TakeProfitPercent / 100);
                    position.StopLoss = quote.AskPrice - (quote.AskPrice * tpm.TakeProfitPercent / 100);


                }
                _positions[strategyId].Add(position);

                if (!tpm.IsBacktest)
                {
                    var message = _strategyOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
            }
        }

        return Task.CompletedTask;
    }

    public async Task StopTest(StrategyMessage message)
    {
        if (message == null || !_strategyProcesses.ContainsKey(message.StrategyId))
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
            _signals.TryRemove(message.StrategyId, out _);

            _ticks.TryRemove(message.StrategyId, out _);
            _logger.LogInformation($"#BT {message.StrategyId} Test stopped");
            await _kafkaProducer.SendMessageAsync(_notificationTopic, message.ToJson());

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in StopTest");
        }
    }

    public List<PositionModel>? GetPositions(Guid strategyId)
    {
        var positions = _positions.ContainsKey(strategyId) ? _positions[strategyId] : null;
        return positions;
    }

    public bool CanHandle(StrategyEnum strategy) =>
         strategy == StrategyEnum.SMA;
}