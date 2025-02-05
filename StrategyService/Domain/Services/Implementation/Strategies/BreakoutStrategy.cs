using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using NuGet.Protocol;
using System.Diagnostics;

namespace BN.PROJECT.StrategyService;

public class BreakoutStrategy : IStrategyService
{
    private readonly ILogger<BreakoutStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStrategyOperations _strategyOperations;
    private readonly IKafkaProducerService _kafkaProducer;

    private readonly ConcurrentDictionary<Guid, List<PositionModel>> _positions = new();
    private readonly ConcurrentDictionary<Guid, BreakoutProcessModel> _strategyProcesses = new();

    public BreakoutStrategy(
        ILogger<BreakoutStrategy> logger,
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
        var breakOutSettings = JsonConvert.DeserializeObject<BreakoutModel>(strategySettings.StrategyParams);

        var tpm = new BreakoutProcessModel
        {
            IsBacktest = message.IsBacktest,
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
            Quantity = strategySettings.Quantity,
            StrategyId = message.StrategyId,
            TakeProfitPercent = strategySettings.TakeProfitPercent,
            StopLossPercent = strategySettings.StopLossPercent,
            TrailingStop = strategySettings.TrailingStop,
            StopLossType = breakOutSettings.StopLossType,
        };
        _ = _strategyProcesses.TryAdd(message.StrategyId, tpm);
        _ = _positions.TryAdd(message.StrategyId, []);
        return Task.CompletedTask;
    }

    public Task EvaluateQuote(Guid strategyId, Guid userId, Quote quote)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!_strategyProcesses.ContainsKey(strategyId))
        {
            return Task.CompletedTask;
        }

        if (quote == null || quote.AskPrice == 0 || quote.BidPrice == 0)
        {
            return Task.CompletedTask;
        }

        var tpm = _strategyProcesses[strategyId];
        var quoteStamp = quote.TimestampUtc;
        if (tpm.StartDate == DateTime.MinValue)
        {
            tpm.StartDate = quoteStamp;
            tpm.TimeFrameStart = _strategyOperations.GetStartOfTimeSpan(quoteStamp, tpm.BreakoutTimeSpan);
            tpm.PrevTimeFrameStart = tpm.TimeFrameStart;
            tpm.CurrentHigh = quote.AskPrice;
            tpm.CurrentLow = quote.BidPrice;
            tpm.CurrentHighStamp = quote.TimestampUtc;
            tpm.CurrentLowStamp = quote.TimestampUtc;
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
                _logger.LogInformation($"#Breakout Limits {tpm.PrevLowStamp:MM.dd HH:mm} {tpm.PrevHighStamp:MM.dd HH:mm}  Low: {tpm.PrevLow}  High: {tpm.PrevHigh}");
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

        var openPosition = _positions[strategyId].Where(p => p.PriceClose == 0).FirstOrDefault();
        if (openPosition == null)
        {
            var breakoutParams = JsonConvert.SerializeObject(new
            {
                PrevLow = tpm.PrevLow,
                PrevHigh = tpm.PrevHigh,
                PrevLowStamp = tpm.PrevLowStamp,
                PrevHighStamp = tpm.PrevHighStamp
            });

            // check breakout high
            if (quote.AskPrice > tpm.PrevHigh && quote.AskPrice - tpm.PrevHigh < 1)
            {
                var sl = tpm.StopLossType == StopLossTypeEnum.None ? quote.BidPrice + (quote.BidPrice * tpm.StopLossPercent / 100) : tpm.PrevLow;
                var position = PositionExtensions.CreatePosition(
                                     tpm.StrategyId,
                                     tpm.Asset,
                                     tpm.Quantity,
                                     SideEnum.Buy,
                                     quote.AskPrice,
                                     sl,
                                     quote.BidPrice + (quote.BidPrice * tpm.TakeProfitPercent / 100),
                                     quote.TimestampUtc,
                                     StrategyEnum.Breakout,
                                     breakoutParams);

                _positions[strategyId].Add(position);
                if (!tpm.IsBacktest)
                {
                    var message = _strategyOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
            }

            // check breakout low
            if (quote.BidPrice < tpm.PrevLow && tpm.PrevLow - quote.BidPrice < 1)
            {
                var sl = tpm.StopLossType == StopLossTypeEnum.None ? quote.AskPrice - (quote.AskPrice * tpm.StopLossPercent / 100) : tpm.PrevHigh;
                var position = PositionExtensions.CreatePosition(
                                   tpm.StrategyId,
                                   tpm.Asset,
                                   tpm.Quantity,
                                   SideEnum.Sell,
                                   quote.AskPrice,
                                   sl,
                                   quote.BidPrice - (quote.BidPrice * tpm.TakeProfitPercent / 100),
                                   quote.TimestampUtc,
                                   StrategyEnum.Breakout,
                                   breakoutParams);

                _positions[strategyId].Add(position);
                if (!tpm.IsBacktest)
                {
                    var message = _strategyOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
            }
        }
        else
        {
            // check if we need to close position at EoD
            if (tpm.AllowOvernight == false && quoteStamp.TimeOfDay > tpm.MarketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                if (!tpm.IsBacktest)
                {
                    var message = _strategyOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
           
            }
            else
            {
                _strategyOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, tpm.TrailingStop, tpm.TakeProfitPercent);
                if (!tpm.IsBacktest)
                {
                    var message = _strategyOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
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
        var strategyId = message.StrategyId;

        if (_strategyProcesses.ContainsKey(strategyId) == false)
        {
            return;
        }
        if (_strategyProcesses[strategyId].IsBacktest)
        {
            var pos = _positions[strategyId].ToList();
            using (var scope = _serviceProvider.CreateScope())
            {
                var strategyRepository = scope.ServiceProvider.GetRequiredService<IStrategyRepository>();
                await strategyRepository.AddPositionsAsync(pos);
            }
        }
        _strategyProcesses.TryRemove(strategyId, out _);
        _positions.TryRemove(strategyId, out _);
        _logger.LogInformation($"#BT {strategyId} Test stopped");
    }

    public List<PositionModel>? GetPositions(Guid strategyId)
    {
        var positions = _positions.ContainsKey(strategyId) ? _positions[strategyId] : null;
        return positions;
    }

    public bool CanHandle(StrategyEnum strategy) =>
     strategy == StrategyEnum.Breakout;
}