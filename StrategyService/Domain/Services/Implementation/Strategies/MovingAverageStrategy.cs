namespace BN.PROJECT.StrategyService;

public class MovingAverageStrategy(

    ILogger<MovingAverageStrategy> logger,
    IKafkaProducerService kafkaProducer) : IStrategyService
{
    private readonly ILogger<MovingAverageStrategy> _logger = logger;
    private readonly IKafkaProducerService _kafkaProducer = kafkaProducer;

    private StrategyTaskEnum _strategyTask;
    private StrategySettingsModel? _strategy;
    private SmaModel? _smaSettings;



    public List<PositionModel> _positions = [];
    private readonly TimeSpan _marketCloseTime = new TimeSpan(19, 55, 0);
    private List<decimal> _prices = [];
    private List<decimal> _longSmas = [];
    private List<decimal> _shortSmas = [];


    private readonly string _notificationTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Notification);
    private readonly string _orderTopic = KafkaUtilities.GetTopicName(KafkaTopicEnum.Order);

    public Task Initialize(StrategyTaskEnum strategyTask, StrategySettingsModel strategySettings)
    {
        if (strategySettings == null)
        {
            return Task.CompletedTask;
        }

        _strategy = strategySettings;
        _strategyTask = strategyTask;

        _smaSettings = JsonConvert.DeserializeObject<SmaModel>(strategySettings.StrategyParams)
                       ?? throw new InvalidOperationException("StrategyParams deserialization returned null.");

        _smaSettings.SlopeWindow = 5;
        _smaSettings.MinSlopeThreshold = 0.05m;

        _logger.LogInformation($"Strategy {strategySettings.Id} initialized with settings: {strategySettings.ToJson()}");

        // Reset state       
        _positions.Clear();
        _prices.Clear();

        // Send notification about strategy initialization
        _kafkaProducer.SendMessageAsync(_notificationTopic, strategySettings.ToJson());

        return Task.CompletedTask;
    }

    public Task EvaluateQuote(Guid strategyId, Guid userId, Quote quote)
    {
        if(_strategy == null || _smaSettings == null)
        {
            return Task.CompletedTask;
        }

        _prices.Add(quote.AskPrice);
        if (_prices.Count <= _smaSettings.LongPeriod)
        {
            return Task.CompletedTask;
        }
        _prices.RemoveAt(0);

        var quoteStamp = quote.TimestampUtc;
        var shortSMA = StrategyOperations.CalculateSMA(_prices, _smaSettings.ShortPeriod);
        var longSMA = StrategyOperations.CalculateSMA(_prices, _smaSettings.LongPeriod);

        _longSmas.Add(longSMA);
        _shortSmas.Add(shortSMA);

        if (_shortSmas.Count <= _smaSettings.SlopeWindow)
        {
            return Task.CompletedTask;
        }

        if (_longSmas.Count > _smaSettings.LongPeriod)
        {
            _longSmas.RemoveAt(0);
            _shortSmas.RemoveAt(0);
        }

        var prevCrossDown = _shortSmas[_shortSmas.Count - 2] < _longSmas[_longSmas.Count - 2];
        var currCrossDown = _shortSmas[_shortSmas.Count - 1] < _longSmas[_longSmas.Count - 1];
        var prevCrossUp = _shortSmas[_shortSmas.Count - 2] > _longSmas[_longSmas.Count - 2];
        var currCrossUp = _shortSmas[_shortSmas.Count - 1] > _longSmas[_longSmas.Count - 1];

        var tradeSignal = TradeSignal.Hold;

        decimal slope = StrategyOperations.CalculateSlope(_shortSmas, _smaSettings.SlopeWindow);

        if (prevCrossDown && currCrossUp && slope > _smaSettings.MinSlopeThreshold)
        {
            tradeSignal = TradeSignal.Buy;
        }
        if (prevCrossUp && currCrossDown && slope < -_smaSettings.MinSlopeThreshold)
        {
            tradeSignal = TradeSignal.Sell;
        }
        //if (prevCrossDown && currCrossUp)
        //{
        //    tradeSignal = TradeSignal.Buy;
        //}
        //if (prevCrossUp && currCrossDown)
        //{
        //    tradeSignal = TradeSignal.Sell;
        //}

        var openPosition = _positions.Where(p => p.PriceClose == 0).FirstOrDefault();


        // check first if we have to handle a openPosition
        if (openPosition != null)
        {
            // check if we need to close position at EoD
            if (_strategy.ClosePositionEod == false && quoteStamp.TimeOfDay > _marketCloseTime)
            {
                var closePrice = openPosition.Side == SideEnum.Buy ? quote.BidPrice : quote.AskPrice;
                openPosition.ClosePosition(quote.TimestampUtc, closePrice, "EoD Close");
            }

            // handle normal SL / TP operation
            else if (_smaSettings.StopLossType == StopLossTypeEnum.None)
            {
                TradingOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, _strategy.TrailingStop, _strategy.TakeProfitPercent);
            
            }

            if (openPosition.PriceClose > 0 && _strategyTask == StrategyTaskEnum.PaperTrade)
            {
                var message = TradingOperations.CreateOrderMessage(strategyId, userId, openPosition).ToJson();
                _kafkaProducer.SendMessageAsync("order", message);
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
                        JsonConvert.SerializeObject(_smaSettings));

            _positions.Add(position);

            if (_strategyTask == StrategyTaskEnum.PaperTrade)
            {
                var message = TradingOperations.CreateOrderMessage(strategyId, userId, position).ToJson();
                _kafkaProducer.SendMessageAsync("order", message);
            }
        }
        return Task.CompletedTask;
    }


    public List<PositionModel> GetPositions()
    {

        return _positions;
    }

    public async Task<TestResult> GetTestResult()
    {
        var testResult = new TestResult();


        testResult.Id = _strategy.Id;
        testResult.Asset = _strategy.Asset;
        testResult.StartDate = _strategy.StartDate;
        testResult.EndDate = DateTime.UtcNow;
        testResult.NumberOfPositions = _positions.Count;
        testResult.NumberOfBuyPositions = _positions.Count(p => p.Side == SideEnum.Buy);
        testResult.NumberOfSellPositions = _positions.Count(p => p.Side == SideEnum.Sell);
        testResult.TotalProfitLoss = _positions.Sum(p => p.ProfitLoss);
        testResult.BuyProfitLoss = _positions.Where(p => p.Side == SideEnum.Buy).Sum(p => p.ProfitLoss);
        testResult.SellProfitLoss = _positions.Where(p => p.Side == SideEnum.Sell).Sum(p => p.ProfitLoss);
        return testResult;
    }

    public bool CanHandle(StrategyEnum strategy)
    {
        return strategy == StrategyEnum.SMA || strategy == StrategyEnum.WMA || strategy == StrategyEnum.EMA;
    }

    public Task StartTest(StrategyMessage message)
    {
        throw new NotImplementedException();
    }
    public async Task StopTest(StrategyMessage message)
    {
        throw new NotImplementedException();
    }
}