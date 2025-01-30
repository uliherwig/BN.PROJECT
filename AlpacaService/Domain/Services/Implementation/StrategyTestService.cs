using System.Diagnostics;

namespace BN.PROJECT.AlpacaService;

public class StrategyTestService : IStrategyTestService
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly IAlpacaTradingService _alpacaTradingService;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly ILogger<StrategyTestService> _logger;

    public StrategyTestService(IAlpacaRepository alpacaRepository,
        ILogger<StrategyTestService> logger,
        IKafkaProducerService kafkaProducer,
        IStrategyServiceClient strategyServiceClient,
        IAlpacaTradingService alpacaTradingService)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _alpacaTradingService = alpacaTradingService;
        _strategyServiceClient = strategyServiceClient;
    }

    public async Task RunBacktest(StrategySettingsModel testSettings)
    {
        var stopwatch = Stopwatch.StartNew();
        var message = new StrategyMessage
        {
            IsBacktest = true,
            StrategyId = testSettings.Id,
            Strategy = testSettings.StrategyType,
            MessageType = MessageTypeEnum.StartTest,
            Settings = testSettings
        };

        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());

        var symbol = testSettings.Asset;
        var startDate = testSettings.StartDate.ToUniversalTime();
        var endDate = testSettings.EndDate.ToUniversalTime();

        // send quotes per day
        TimeSpan timeFrame = TimeSpan.FromDays(1);

        var stamp = startDate.ToUniversalTime();

        while (stamp < endDate)
        {
            var stampEnd = stamp.Add(timeFrame).ToUniversalTime();
            var bars = await _alpacaRepository.GetHistoricalBars(symbol, stamp, stampEnd);
            if (bars.Count == 0)
            {
                stamp = stamp.Add(timeFrame).ToUniversalTime();
                continue;
            }
            var quotesDay = new List<Quote>();
            foreach (var bar in bars)
            {
                var q = new Quote
                {
                    Symbol = symbol,
                    AskPrice = bar.C + 0.1m,
                    BidPrice = bar.C - 0.1m,
                    TimestampUtc = bar.T.ToUniversalTime()
                };
                quotesDay.Add(q);
            }
            message.MessageType = MessageTypeEnum.Quotes;
            message.Settings = null;
            message.Quotes = quotesDay;

            await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
            _logger.LogInformation($"RunBacktest {stamp.ToLocalTime()} current time {stopwatch.ElapsedMilliseconds} ms");
            stamp = stamp.Add(timeFrame).ToUniversalTime();
        };

        message.MessageType = MessageTypeEnum.StopTest;
        message.Settings = null;
        message.Quotes = null;

        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());

        stopwatch.Stop();
        _logger.LogInformation($"RunBacktest {testSettings.Id} completed in {stopwatch.ElapsedMilliseconds} ms");
    }
    public async Task RunExecution(Guid userId, Guid strategyId)
    {
        var testSettings = await _strategyServiceClient.GetStrategyAsync(strategyId.ToString());
        if (testSettings == null)
        {
            _logger.LogError($"RunExecution: Strategy {strategyId} not found");
            return;
        }
        var message = new StrategyMessage
        {
            IsBacktest = false,
            UserId = userId,
            StrategyId = testSettings.Id,
            Strategy = testSettings.StrategyType,
            MessageType = MessageTypeEnum.StartTest,
            Settings = testSettings
        };

        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());

        var symbol = testSettings.Asset;
        var startDate = testSettings.StartDate.ToUniversalTime();
        var endDate = testSettings.EndDate.ToUniversalTime();

        // send quotes per day
        TimeSpan timeFrame = TimeSpan.FromDays(1);

        var stamp = startDate.ToUniversalTime();

        while (stamp < endDate)
        {
            var stampEnd = stamp.Add(timeFrame).ToUniversalTime();
            var bars = await _alpacaRepository.GetHistoricalBars(symbol, stamp, stampEnd);
            if (bars.Count == 0)
            {
                stamp = stamp.Add(timeFrame).ToUniversalTime();
                continue;
            }
            var quotesDay = new List<Quote>();
            foreach (var bar in bars)
            {
                var q = new Quote
                {
                    Symbol = symbol,
                    AskPrice = bar.C + 0.1m,
                    BidPrice = bar.C - 0.1m,
                    TimestampUtc = bar.T.ToUniversalTime()
                };
                quotesDay.Add(q);
            }
            message.MessageType = MessageTypeEnum.Quotes;
            message.Settings = null;
            message.Quotes = quotesDay;

            await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
            stamp = stamp.Add(timeFrame).ToUniversalTime();
        };

        message.MessageType = MessageTypeEnum.StopTest;
        message.Settings = null;
        message.Quotes = null;

        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());

    }


    public async Task CreateAlpacaOrder(OrderMessage orderMessage)
    {
        _logger.LogInformation($"Run Alpaca Execution");


        var userId = orderMessage.UserId;
        var symbol = orderMessage.Position.Symbol;
        var qty = orderMessage.Position.Quantity;
        var side = orderMessage.Position.Side == SideEnum.Buy ? OrderSide.Buy : OrderSide.Sell;
        var orderType = OrderType.Market;
        var timeInForce = TimeInForce.Day;

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId.ToString());
        if (userSettings == null)
        {
            _logger.LogError("User settings not found for userId: {userId}");
            return;
        }

        await _alpacaTradingService.CreateOrderAsync(userSettings, symbol, qty, side, orderType, timeInForce);

    }

    public async Task StopExecution(Guid userId, Guid strategyId)
    {
        var message = new StrategyMessage
        {
            MessageType = MessageTypeEnum.StopTest,
            IsBacktest = false,
            UserId = userId,
            StrategyId = strategyId

        };
        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
    }
}