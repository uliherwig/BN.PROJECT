
namespace BN.PROJECT.StrategyService;

public class EmaStrategy : IStrategyService
{
    private readonly ILogger<EmaStrategy> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IKafkaProducerService _kafkaProducer;

    public List<PositionModel> _positions = new();
    private SmaProcessModel _strategy;
    private List<SmaTick> _ticks = new();
    private List<SideEnum> _signals = new();

    private readonly string _notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);
    private readonly string _orderTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Order);

    public EmaStrategy(

        ILogger<EmaStrategy> logger,
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

        _strategy = new SmaProcessModel
        {
            IsBacktest = message.IsBacktest,
            StrategyId = message.StrategyId,
            StartDate = DateTime.MinValue,
            Asset = strategySettings.Asset,
            Quantity = strategySettings.Quantity,
            AllowOvernight = strategySettings.ClosePositionEod,
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
            ShortPeriod = _strategy.ShortPeriod,
            LongPeriod = _strategy.LongPeriod,
            StopLossType = _strategy.StopLossType
        });
        var quoteStamp = quote.TimestampUtc;
        var currentSmaTick = new SmaTick
        {
            StrategyId = strategyId,
            AskPrice = quote.AskPrice,
            BidPrice = quote.BidPrice,
            Asset = _strategy.Asset,
            TimestampUtc = quote.TimestampUtc,
            ShortSma = 0,
            LongSma = 0
        };

        _strategy.LastQuotes.Add(quote);
        if (_strategy.LastQuotes.Count <= _strategy.LongPeriod)
        {
            return Task.CompletedTask;
        }
        _strategy.LastQuotes.RemoveAt(0);


        var shortPeriodQuotes = _strategy.LastQuotes.Skip(_strategy.LongPeriod - _strategy.ShortPeriod).Take(_strategy.ShortPeriod).ToList();
        var longPeriodQuotes = _strategy.LastQuotes;

        var longEma = longPeriodQuotes.Average(q => q.AskPrice);
        decimal alpha = (decimal)(2.0 / (_strategy.LongPeriod + 1));
        for (int i = 0; i < longPeriodQuotes.Count; i++)
        {
            longEma = alpha * longPeriodQuotes[i].AskPrice + (1 - alpha) * longEma;
        }
        
        var shortSma = shortPeriodQuotes.Average(q => q.AskPrice);
        alpha = (decimal)(2.0 / (_strategy.ShortPeriod + 1));
        for (int i = 0; i < shortPeriodQuotes.Count; i++)
        {
            shortSma = alpha * shortPeriodQuotes[i].AskPrice + (1 - alpha) * shortSma;
        }

        var order = false;

        var signal = shortSma > longEma ? SideEnum.Buy : SideEnum.Sell;
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
        currentSmaTick.LongSma = longEma;
        _ticks.Add(currentSmaTick);

        _strategy.LastSmas.Add(currentSmaTick);

        var openPosition = _positions.Where(p => p.PriceClose == 0).FirstOrDefault();


        // check first if we have to handle a openPosition
        if (openPosition != null)
        {
            // check if we need to close position at EoD
            if (_strategy.AllowOvernight == false && quoteStamp.TimeOfDay > _strategy.MarketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
                return Task.CompletedTask;
            }

            // handle normal SL / TP operation
            if (_strategy.StopLossType == StopLossTypeEnum.None)
            {
                TradingOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, _strategy.TrailingStop, _strategy.TakeProfitPercent);
            }

            if (_strategy.StopLossType == StopLossTypeEnum.SMAIntersection && order)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "MA");

            }
            var test = _positions.Where(p => p.PriceClose == 0).FirstOrDefault();
            if (openPosition.PriceClose > 0 && !_strategy.IsBacktest)
            {
                var message = TradingOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                _kafkaProducer.SendMessageAsync("order", message);
            }
        }
        if (_positions.Where(p => p.PriceClose == 0).FirstOrDefault() == null)
        {
            if (order)
            {
                var position = PositionExtensions.CreatePosition(
                              _strategy.StrategyId,
                              _strategy.Asset,
                              _strategy.Quantity,
                              SideEnum.Buy,
                              quote.AskPrice,
                              quote.BidPrice - (quote.BidPrice * _strategy.TakeProfitPercent / 100),
                              quote.BidPrice + (quote.BidPrice * _strategy.TakeProfitPercent / 100),
                              quote.TimestampUtc,
                              StrategyEnum.SMA,
                              smaParams);
                if (_signals.ElementAt(0) == SideEnum.Sell)
                {
                    position.PriceOpen = quote.BidPrice;
                    position.Side = SideEnum.Sell;
                    position.TakeProfit = quote.AskPrice + (quote.AskPrice * _strategy.TakeProfitPercent / 100);
                    position.StopLoss = quote.AskPrice - (quote.AskPrice * _strategy.TakeProfitPercent / 100);
                }
                _positions.Add(position);

                if (!_strategy.IsBacktest)
                {
                    var message = TradingOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
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

    
        testResult.Id = _strategy.StrategyId;
        testResult.Asset = _strategy.Asset;
        testResult.StartDate = _strategy.StartDate;
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
     strategy == StrategyEnum.MeanReversion;

    public Task Initialize(StrategyTaskEnum strategyTask, StrategySettingsModel strategySettings)
    {
        throw new NotImplementedException();
    }
}