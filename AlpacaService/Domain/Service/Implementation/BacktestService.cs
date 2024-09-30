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
        var topic = $"backtest-{testSettings.UserEmail.ToLower().Replace('@', '-').Replace('.', '-')}";
      


        var result = await _strategyServiceClient.StartStrategyAsync(testSettings);

        if (result != "true")
        {
            return "Error";
        }
        await _kafkaProducer.SendMessageAsync(topic, "start");

        var symbol = testSettings.Symbol;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = DateTime.UtcNow;
        var timeFrame = TimeSpan.FromDays(1);

        var stamp = startDate.ToUniversalTime(); 

        while (stamp < endDate)
        {
            var bars = await _alpacaRepository.GetHistoricalBars(symbol, stamp, stamp.Add(timeFrame));
            if (bars.Count == 0)
            {
                stamp = stamp.Add(timeFrame).ToUniversalTime();
                continue;
            }
        
            foreach (var bar in bars)
            {
                var q = new Quote
                {
                    Symbol = symbol,
                    AskPrice = bar.C + 0.1m,
                    BidPrice = bar.C - 0.1m,
                    TimestampUtc = bar.T.ToUniversalTime()
                };
               

                await _kafkaProducer.SendMessageAsync(topic, q.ToJson());
            }
            stamp = stamp.Add(timeFrame).ToUniversalTime();
        };
        await _kafkaProducer.SendMessageAsync(topic, "stop");
        return "OK";
    }
}
