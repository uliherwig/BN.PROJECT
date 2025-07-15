namespace BN.PROJECT.StrategyService;

public class SmaStrategy : IStrategyService
{
    private readonly ILogger<SmaStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IKafkaProducerService _kafkaProducer;

    public List<PositionModel> _positions = new();
    private SmaProcessModel _strategyProcess;
    private List<SmaTick> _ticks = new();
    private List<SideEnum> _signals = new();

    private readonly string _notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);
    private readonly string _orderTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Order);

    public SmaStrategy(
        ILogger<SmaStrategy> logger,
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
        var smaSettings = JsonConvert.DeserializeObject<SmaModel>(strategySettings.StrategyParams);
        if (smaSettings == null)
        {
            return Task.CompletedTask;
        }

        _strategyProcess = new SmaProcessModel
        {
            IsBacktest = message.IsBacktest,
            StrategyId = message.StrategyId,
            StartDate = DateTime.MinValue,
            Asset = strategySettings.Asset,
            Quantity = strategySettings.Quantity,
            AllowOvernight = strategySettings.AllowOvernight,
            TakeProfitPercent = strategySettings.TakeProfitPercent,
            StopLossPercent = strategySettings.StopLossPercent,
            TrailingStop = strategySettings.TrailingStop,
            ShortPeriod = smaSettings.ShortPeriod,
            LongPeriod = smaSettings.LongPeriod,
            StopLossType = smaSettings.StopLossType,
            MarketCloseTime = new TimeSpan(19, 55, 0),
        }; 

        _kafkaProducer.SendMessageAsync(_notificationTopic, message.ToJson());


        return Task.CompletedTask;
    }

    public Task EvaluateQuote(Guid strategyId, Guid userId, Quote quote)
    {
        // how parameters are handled 
        // check signal short > long SMA => Buy/Sell
        // check close method 
        // 1. intersection - means close and open signal at the same time
        // 2. stop loss - means close position if price is below stop loss
        // 3. take profit - means close position if price is above take profit
        // sl and tp must then be set both
        // 4. EoD - means close position at the end of the day 

        var smaParams = JsonConvert.SerializeObject(new
        {
            ShortPeriod = _strategyProcess.ShortPeriod,
            LongPeriod = _strategyProcess.LongPeriod,
            StopLossType = _strategyProcess.StopLossType
        });
        var quoteStamp = quote.TimestampUtc;
        var currentSmaTick = new SmaTick
        {
            StrategyId = strategyId,
            AskPrice = quote.AskPrice,
            BidPrice = quote.BidPrice,
            Asset = _strategyProcess.Asset,
            TimestampUtc = quote.TimestampUtc,
            ShortSma = 0,
            LongSma = 0
        };

        _strategyProcess.LastQuotes.Add(quote);
        if (_strategyProcess.LastQuotes.Count <= _strategyProcess.LongPeriod)
        {
            return Task.CompletedTask;
        }
        _strategyProcess.LastQuotes.RemoveAt(0);

        var longSma = _strategyProcess.LastQuotes.Average(q => q.AskPrice);
        var shortSma = _strategyProcess.LastQuotes.Skip(_strategyProcess.LongPeriod - _strategyProcess.ShortPeriod).Take(_strategyProcess.ShortPeriod).Average(q => q.AskPrice);

        var order = false;

        var signal = shortSma > longSma ? SideEnum.Buy : SideEnum.Sell;
        _signals.Add(signal);
        if (_signals.Count == 2)
        {
            order = _signals.ElementAt(0) != _signals.ElementAt(1);
            if (order)
            {
                // _logger.LogInformation($"Signals : {_signals.ElementAt(0)} {_signals.ElementAt(1)}");
            }

            _signals.RemoveAt(0);
        }
        // _logger.LogInformation($"SMA Intersection detected for strategy {strategyId} at {quoteStamp}; signal = {_signals.ElementAt(0)}");


        currentSmaTick.ShortSma = shortSma;
        currentSmaTick.LongSma = longSma;
        _ticks.Add(currentSmaTick);

        _strategyProcess.LastSmas.Add(currentSmaTick);

        var openPosition = _positions.Where(p => p.PriceClose == 0).FirstOrDefault();


        // check first if we have to handle a openPosition
        if (openPosition != null)
        {
            // check if we need to close position at EoD
            if (_strategyProcess.AllowOvernight == false && quoteStamp.TimeOfDay > _strategyProcess.MarketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
                return Task.CompletedTask;
            }

            // handle normal SL / TP operation
            if (_strategyProcess.StopLossType == StopLossTypeEnum.None)
            {
                StrategyOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, _strategyProcess.TrailingStop, _strategyProcess.TakeProfitPercent);
            }

            if (_strategyProcess.StopLossType == StopLossTypeEnum.SMAIntersection && order)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "MA");

            }
            var test = _positions.Where(p => p.PriceClose == 0).FirstOrDefault();
            if (openPosition.PriceClose > 0 && !_strategyProcess.IsBacktest)
            {
                var message = StrategyOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                _kafkaProducer.SendMessageAsync("order", message);
            }
        }
        if (_positions.Where(p => p.PriceClose == 0).FirstOrDefault() == null)
        {
            if (order)
            {
                var position = PositionExtensions.CreatePosition(
                              _strategyProcess.StrategyId,
                              _strategyProcess.Asset,
                              _strategyProcess.Quantity,
                              SideEnum.Buy,
                              quote.AskPrice,
                              quote.BidPrice - (quote.BidPrice * _strategyProcess.TakeProfitPercent / 100),
                              quote.BidPrice + (quote.BidPrice * _strategyProcess.TakeProfitPercent / 100),
                              quote.TimestampUtc,
                              StrategyEnum.SMA,
                              smaParams);
                if (_signals.ElementAt(0) == SideEnum.Sell)
                {
                    position.PriceOpen = quote.BidPrice;
                    position.Side = SideEnum.Sell;
                    position.TakeProfit = quote.AskPrice + (quote.AskPrice * _strategyProcess.TakeProfitPercent / 100);
                    position.StopLoss = quote.AskPrice - (quote.AskPrice * _strategyProcess.TakeProfitPercent / 100);
                }
                _positions.Add(position);

                if (!_strategyProcess.IsBacktest)
                {
                    var message = StrategyOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                    _kafkaProducer.SendMessageAsync("order", message);
                }
            }
        }

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

    
        testResult.Id = _strategyProcess.StrategyId;
        testResult.Asset = _strategyProcess.Asset;
        testResult.StartDate = _strategyProcess.StartDate;
        testResult.EndDate = DateTime.UtcNow;
        testResult.NumberOfPositions =  _positions.Count;
        testResult.NumberOfBuyPositions = _positions.Count(p => p.Side == SideEnum.Buy);
        testResult.NumberOfSellPositions = _positions.Count(p => p.Side == SideEnum.Sell);
        testResult.TotalProfitLoss =  _positions.Sum(p => p.ProfitLoss);
        testResult.BuyProfitLoss = _positions.Where(p => p.Side == SideEnum.Buy).Sum(p => p.ProfitLoss);
        testResult.SellProfitLoss = _positions.Where(p => p.Side == SideEnum.Sell).Sum(p => p.ProfitLoss);      
        return testResult;
    }

    public bool CanHandle(StrategyEnum strategy) =>
     strategy == StrategyEnum.SMA;
}