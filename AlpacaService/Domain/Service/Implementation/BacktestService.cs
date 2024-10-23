using NuGet.Protocol;

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

    public async Task<string> RunBacktest(BacktestSettings testSettings)
    {
        var message = new StrategyMessage
        {

            TestId = testSettings.Id,
            Type = MessageType.StartTest,
            Quotes = null,
            TestSettings = testSettings
        };
        var m = message.ToJson();

        await _kafkaProducer.SendMessageAsync("strategy", m);

        var symbol = testSettings.Symbol;
        var startDate = testSettings.StartDate.ToUniversalTime();
        var endDate = testSettings.EndDate.ToUniversalTime();


        // send quotes per day // breakout period is handled in strategy service
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
            message = new StrategyMessage
            {

                TestId = testSettings.Id,
                Type = MessageType.Quotes,
                Quotes = quotesDay
            };

            await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());

            stamp = stamp.Add(timeFrame).ToUniversalTime();
        };


        message = new StrategyMessage
        {

            TestId = testSettings.Id,
            Type = MessageType.StopTest,
            Quotes = null
        };

        await _kafkaProducer.SendMessageAsync("strategy", message.ToJson());
        return "OK";
    }
}
