using System.Diagnostics;

namespace BN.PROJECT.StrategyService;

public class BreakoutStrategy : IStrategyService
{
    private readonly ILogger<BreakoutStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IKafkaProducerService _kafkaProducer;

    private List<PositionModel> _positions = new();
    private BreakoutProcessModel _strategyProcess;

    private readonly string _notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);


    public BreakoutStrategy(
        ILogger<BreakoutStrategy> logger,
        IServiceProvider serviceProvider,
        IKafkaProducerService kafkaProducer)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
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

         _strategyProcess = new BreakoutProcessModel
        {
            IsBacktest = message.IsBacktest,
            StartDate = DateTime.MinValue,
            BreakoutTimeSpan = StrategyOperations.GetTimeSpanByBreakoutPeriod(breakOutSettings.BreakoutPeriod),
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
  
       
        return Task.CompletedTask;
    }

    public Task EvaluateQuote(Guid strategyId, Guid userId, Quote quote)
    {
        var stopwatch = Stopwatch.StartNew();  

        if (quote == null || quote.AskPrice == 0 || quote.BidPrice == 0)
        {
            return Task.CompletedTask;
        }

        var quoteStamp = quote.TimestampUtc;
        if (_strategyProcess.StartDate == DateTime.MinValue)
        {
            _strategyProcess.StartDate = quoteStamp;
            _strategyProcess.TimeFrameStart = StrategyOperations.GetStartOfTimeSpan(quoteStamp, _strategyProcess.BreakoutTimeSpan);
            _strategyProcess.PrevTimeFrameStart = _strategyProcess.TimeFrameStart;
            _strategyProcess.CurrentHigh = quote.AskPrice;
            _strategyProcess.CurrentLow = quote.BidPrice;
            _strategyProcess.CurrentHighStamp = quote.TimestampUtc;
            _strategyProcess.CurrentLowStamp = quote.TimestampUtc;
        }
        _strategyProcess.TimeFrameStart = StrategyOperations.GetStartOfTimeSpan(quoteStamp, _strategyProcess.BreakoutTimeSpan);

        if (_strategyProcess.TimeFrameStart > _strategyProcess.PrevTimeFrameStart)
        {
            _strategyProcess.LastTimeFrameVolume++;
            _strategyProcess.PrevTimeFrameStart = _strategyProcess.TimeFrameStart;

            if (_strategyProcess.CurrentHigh > _strategyProcess.PrevHigh || _strategyProcess.CurrentLow < _strategyProcess.PrevLow || _strategyProcess.LastTimeFrameVolume < 2)
            {
                _strategyProcess.PrevHigh = _strategyProcess.CurrentHigh;
                _strategyProcess.PrevHighStamp = _strategyProcess.CurrentHighStamp;
                _strategyProcess.PrevLow = _strategyProcess.CurrentLow;
                _strategyProcess.PrevLowStamp = _strategyProcess.CurrentLowStamp;
                // _logger.LogInformation($"#Breakout Limits {_strategyProcess.PrevLowStamp:MM.dd HH:mm} {_strategyProcess.PrevHighStamp:MM.dd HH:mm}  Low: {_strategyProcess.PrevLow}  High: {_strategyProcess.PrevHigh}");
            }

            _strategyProcess.TakeProfitPlus = (_strategyProcess.PrevHigh - _strategyProcess.PrevLow) / 2;
            _strategyProcess.CurrentHigh = quote.AskPrice;
            _strategyProcess.CurrentHighStamp = quote.TimestampUtc;
            _strategyProcess.CurrentLow = quote.BidPrice;
            _strategyProcess.CurrentLowStamp = quote.TimestampUtc;
        }

        if (quoteStamp > _strategyProcess.TimeFrameStart && quoteStamp < _strategyProcess.TimeFrameStart.Add(_strategyProcess.BreakoutTimeSpan))
        {
            if (quote.AskPrice > _strategyProcess.CurrentHigh)
            {
                _strategyProcess.CurrentHigh = quote.AskPrice;
                _strategyProcess.CurrentHighStamp = quote.TimestampUtc;
            }
            if (quote.BidPrice < _strategyProcess.CurrentLow)
            {
                _strategyProcess.CurrentLow = quote.BidPrice;
                _strategyProcess.CurrentLowStamp = quote.TimestampUtc;
            }
        }

        // not ready to start trading
        if (_strategyProcess.PrevLow == 0)
        {
            return Task.CompletedTask;
        }

        var openPosition = _positions.Where(p => p.PriceClose == 0).FirstOrDefault();
        if (openPosition == null)
        {
            var breakoutParams = JsonConvert.SerializeObject(new
            {
                PrevLow = _strategyProcess.PrevLow,
                PrevHigh = _strategyProcess.PrevHigh,
                PrevLowStamp = _strategyProcess.PrevLowStamp,
                PrevHighStamp = _strategyProcess.PrevHighStamp
            });

            // check breakout high
            if (quote.AskPrice > _strategyProcess.PrevHigh && quote.AskPrice - _strategyProcess.PrevHigh < 1)
            {
                var sl = _strategyProcess.StopLossType == StopLossTypeEnum.None ? quote.BidPrice - (quote.BidPrice * _strategyProcess.StopLossPercent / 100) : _strategyProcess.PrevLow;
                var position = PositionExtensions.CreatePosition(
                                     _strategyProcess.StrategyId,
                                     _strategyProcess.Asset,
                                     _strategyProcess.Quantity,
                                     SideEnum.Buy,
                                     quote.AskPrice,
                                     sl,
                                     quote.BidPrice + (quote.BidPrice * _strategyProcess.TakeProfitPercent / 100),
                                     quote.TimestampUtc,
                                     StrategyEnum.Breakout,
                                     breakoutParams);

                _positions.Add(position);
                if (!_strategyProcess.IsBacktest)
                {
                    var message = StrategyOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
            }

            // check breakout low
            if (quote.BidPrice < _strategyProcess.PrevLow && _strategyProcess.PrevLow - quote.BidPrice < 1)
            {
                var sl = _strategyProcess.StopLossType == StopLossTypeEnum.None ? quote.AskPrice + (quote.AskPrice * _strategyProcess.StopLossPercent / 100) : _strategyProcess.PrevHigh;
                var position = PositionExtensions.CreatePosition(
                                   _strategyProcess.StrategyId,
                                   _strategyProcess.Asset,
                                   _strategyProcess.Quantity,
                                   SideEnum.Sell,
                                   quote.BidPrice,
                                   sl,
                                   quote.AskPrice - (quote.AskPrice * _strategyProcess.TakeProfitPercent / 100),
                                   quote.TimestampUtc,
                                   StrategyEnum.Breakout,
                                   breakoutParams);

                _positions.Add(position);
                if (!_strategyProcess.IsBacktest)
                {
                    var message = StrategyOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
            }
        }
        else
        {
            // check if we need to close position at EoD
            if (_strategyProcess.AllowOvernight == false && quoteStamp.TimeOfDay > _strategyProcess.MarketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                if (!_strategyProcess.IsBacktest)
                {
                    var message = StrategyOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
           
            }
            else
            {
                StrategyOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, _strategyProcess.TrailingStop, _strategyProcess.TakeProfitPercent);
                if (!_strategyProcess.IsBacktest)
                {
                    var message = StrategyOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
            }
        }
        stopwatch.Stop();
        //_logger.LogInformation($"Breakout EvaluateQuote done in {stopwatch.Elapsed.TotalNanoseconds} ns");
        return Task.CompletedTask;
    }

    public async Task StopTest(StrategyMessage message)
    {

 
    }

    public List<PositionModel> GetPositions()
    {
        return _positions;
    }
    public async Task<TestResult> GetTestResult()
    {
        var testResult = new TestResult();
        testResult.EndDate = DateTime.UtcNow;
        testResult.NumberOfPositions = _positions.Count();
        testResult.NumberOfBuyPositions = _positions.Count(p => p.Side == SideEnum.Buy);
        testResult.NumberOfSellPositions = _positions.Count(p => p.Side == SideEnum.Sell);
        testResult.TotalProfitLoss = _positions.Sum(p => p.ProfitLoss);
        testResult.BuyProfitLoss = _positions.Where(p => p.Side == SideEnum.Buy).Sum(p => p.ProfitLoss);
        testResult.SellProfitLoss = _positions.Where(p => p.Side == SideEnum.Sell).Sum(p => p.ProfitLoss);    
        return testResult;
    }

    public bool CanHandle(StrategyEnum strategy) =>
     strategy == StrategyEnum.Breakout;

 
}