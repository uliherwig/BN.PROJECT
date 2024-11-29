using System.Diagnostics;

namespace BN.PROJECT.AlpacaService;

public class BacktestService : IBacktestService
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IStrategyServiceClient _strategyServiceClient;
    private readonly IKafkaProducerHostedService _kafkaProducer;
    private readonly ILogger<BacktestService> _logger;

    public BacktestService(IAlpacaRepository alpacaRepository,
        ILogger<BacktestService> logger,
        IKafkaProducerHostedService kafkaProducer,
        IStrategyServiceClient strategyServiceClient)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _strategyServiceClient = strategyServiceClient;
    }
    public async Task<string> RunBacktest(StrategySettingsModel testSettings)
    {
        var stopwatch = Stopwatch.StartNew();
        var message = new StrategyMessage
        {
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
        return "OK";
    }

}