using System.Diagnostics;


namespace BN.PROJECT.StrategyService;

public class BreakoutStrategy : IStrategyService
{
    private readonly ILogger<BreakoutStrategy> _logger;
    private readonly IRedisPublisher _publisher;

    private StrategyTaskEnum _strategyTask;
    private StrategySettingsModel? _strategy;
    private BreakoutModel? _breakoutSettings;


    private List<Quote> _quotes = [];
    private decimal _prevHigh = 0.0m;
    private decimal _prevLow = 0.0m;


    public List<PositionModel> _positions = [];
    private readonly TimeSpan _marketCloseTime = new TimeSpan(19, 55, 0);

    private readonly string _notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);
    private readonly string _orderTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Order);


    public BreakoutStrategy(

        ILogger<BreakoutStrategy> logger,
        IRedisPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    //public Task StartTest(StrategyMessage message)
    //{
    //    if (message == null || message.Settings == null)
    //    {
    //        return Task.CompletedTask;
    //    }
    //    var strategySettings = message.Settings;
    //    var breakOutSettings = JsonConvert.DeserializeObject<BreakoutModel>(strategySettings.StrategyParams);

    //     _strategy = new BreakoutProcessModel
    //    {
    //        IsBacktest = message.IsBacktest,
    //        StartDate = DateTime.MinValue,
    //        BreakoutTimeSpan = StrategyOperations.GetTimeSpanByBreakoutPeriod(breakOutSettings.BreakoutPeriod),
    //        CurrentHigh = 10000.0m,
    //        CurrentLow = 0.0m,
    //        PrevHigh = 10000.0m,
    //        PrevLow = 0.0m,
    //        LastTimeFrameVolume = 0,  // in use but senseless
    //        TimeFrameStart = DateTime.MinValue,
    //        PrevTimeFrameStart = DateTime.MinValue,
    //        TakeProfitPlus = 0.0m, // currently obsolete
    //        MarketCloseTime = new TimeSpan(19, 55, 0), // todo replace with method to get market close time get it from asset
    //        AllowOvernight = strategySettings.AllowOvernight,
    //        Asset = strategySettings.Asset,
    //        Quantity = strategySettings.Quantity,
    //        StrategyId = message.StrategyId,
    //        TakeProfitPercent = strategySettings.TakeProfitPercent,
    //        StopLossPercent = strategySettings.StopLossPercent,
    //        TrailingStop = strategySettings.TrailingStop,
    //        StopLossType = breakOutSettings.StopLossType,
    //    };


    //    return Task.CompletedTask;
    //}

    public Task Initialize(StrategyTaskEnum strategyTask, StrategySettingsModel strategySettings)
    {
        if (strategySettings == null)
        {
            return Task.CompletedTask;
        }

        _strategy = strategySettings;
        _strategyTask = strategyTask;

        _breakoutSettings = JsonConvert.DeserializeObject<BreakoutModel>(strategySettings.StrategyParams)
                       ?? throw new InvalidOperationException("StrategyParams deserialization returned null.");



        _logger.LogInformation($"Strategy {strategySettings.Id} initialized with settings: {strategySettings.ToJson()}");

        // Reset state       
        _positions.Clear();

        // Send notification about strategy initialization
        //_kafkaProducer.SendMessageAsync(_notificationTopic, strategySettings.ToJson());
        _publisher.Publish(_notificationTopic, strategySettings.ToJson());

        return Task.CompletedTask;
    }

    public Task EvaluateQuote(Guid strategyId, Guid userId, Quote quote)
    {
        var stopwatch = Stopwatch.StartNew();

        if (_strategy == null || _breakoutSettings == null)
        {
            return Task.CompletedTask;
        }

        if (quote == null || quote.AskPrice == 0 || quote.BidPrice == 0)
        {
            return Task.CompletedTask;
        }

        _quotes.Add(quote);

        var timeSpan = StrategyOperations.GetTimeSpanByBreakoutPeriod(_breakoutSettings.BreakoutPeriod);

        var minutes = timeSpan.TotalMinutes;

        var startOfTimeSpan = StrategyOperations.GetStartOfTimeSpan(quote.TimestampUtc, timeSpan);
        var prevStartOfTimeSpan = startOfTimeSpan.Add(-timeSpan);
        if (_quotes.Count <= timeSpan.TotalMinutes * 2)
        {
            return Task.CompletedTask;
        }


        var quotesPrevPeriod = _quotes
            .Where(q => q.TimestampUtc >= prevStartOfTimeSpan && q.TimestampUtc < startOfTimeSpan)
            .ToList();

        if (quotesPrevPeriod.Count < 10)
        {
            _logger.LogWarning($"No quotes found for the time span starting at {startOfTimeSpan}.");
            return Task.CompletedTask;
        }

        var maxAskQuote = quotesPrevPeriod
            .OrderByDescending(q => q.AskPrice)
            .First();
        var minBidQuote = quotesPrevPeriod
            .OrderBy(q => q.BidPrice)
            .First();


        var tradeSignal = TradeSignal.Hold;

        // ask > prev high
        if (quote.AskPrice > maxAskQuote.AskPrice)
        {
            tradeSignal = TradeSignal.Buy;
        }
        // bid < prev low
        if (quote.BidPrice < minBidQuote.BidPrice)
        {
            tradeSignal = TradeSignal.Sell;
        }

        var breakoutParams = JsonConvert.SerializeObject(new
        {
            PrevLow = minBidQuote.BidPrice,
            PrevHigh = maxAskQuote.AskPrice,
            PrevLowStamp = minBidQuote.TimestampUtc,
            PrevHighStamp = maxAskQuote.TimestampUtc

        });

        var openPosition = _positions.Where(p => p.PriceClose == 0).FirstOrDefault();


        // check first if we have to handle a openPosition
        if (openPosition != null)
        {
            // check if we need to close position at EoD
            if (_strategy.ClosePositionEod == false && quote.TimestampUtc.TimeOfDay > _marketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
            }

            // handle normal SL / TP operation
            else if (_breakoutSettings.StopLossType == StopLossTypeEnum.None)
            {
                TradingOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, _strategy.TrailingStop, _strategy.TakeProfitPercent);

            }

            if (openPosition.PriceClose > 0 && _strategyTask == StrategyTaskEnum.PaperTrade)
            {
                var message = TradingOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                //_kafkaProducer.SendMessageAsync("order", message);
                _publisher.Publish("order",message);

            }
        }
        else
        {

            if (tradeSignal == TradeSignal.Hold)
            {
                return Task.CompletedTask;
            }

            var side = tradeSignal == TradeSignal.Buy ? SideEnum.Buy : SideEnum.Sell;
            var priceOpen = side == SideEnum.Buy ? quote.AskPrice : quote.BidPrice;
            var tp = side == SideEnum.Buy
                ? quote.BidPrice + (quote.BidPrice * _strategy.TakeProfitPercent / 100)
                : quote.AskPrice - (quote.AskPrice * _strategy.TakeProfitPercent / 100);
            var sl = side == SideEnum.Buy
                ? quote.BidPrice - (quote.BidPrice * _strategy.StopLossPercent / 100)
                : quote.AskPrice + (quote.AskPrice * _strategy.StopLossPercent / 100);

            var position = PositionExtensions.CreatePosition(
                        _strategy.Id,
                        _strategy.Asset,
                        _strategy.Quantity,
                        side,
                        priceOpen,
                        sl,
                        tp,
                        quote.TimestampUtc,
                        _strategy.StrategyType,
                        JsonConvert.SerializeObject(_breakoutSettings));

            _positions.Add(position);

            if (_strategyTask == StrategyTaskEnum.PaperTrade)
            {
                var message = TradingOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                //_kafkaProducer.SendMessageAsync("order", message);
                _publisher.Publish("order",message);

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


    public Task StartTest(StrategyMessage message)
    {
        throw new NotImplementedException();
    }
}